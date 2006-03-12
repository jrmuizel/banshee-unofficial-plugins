/***************************************************************************
 *  helix-dbus-server.cc
 *
 *  Copyright (C) 2006 Novell, Inc
 *  Written by Aaron Bockover <aaron@abock.org>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
#include <stdlib.h>
#include <stdio.h>
#include <unistd.h>
#include <glib.h>
#include <glib/gstdio.h>
#include <dbus/dbus.h>
#include <dbus/dbus-glib.h>

#include "hxplayer.h"
#include "hxmessage.h"
#include "hxmessagesegment.h"

#include "helix-dbus-server.h"

// private definitions

struct HelixDbusServer {
    DBusConnection *connection;
    HxPlayer *player;
};

typedef void (* HelixDbusMethodHandler) (HelixDbusServer *server, 
    DBusMessage *message, void **return_value);

typedef struct HelixDbusMethodVTable HelixDbusMethodVTable;

struct HelixDbusMethodVTable {
    const gchar *method_name;
    int return_type;
    HelixDbusMethodHandler handler;
};

#define METHOD_HANDLER_DEFINE(name) static void (name)(HelixDbusServer *server, \
    DBusMessage *message, void **return_value);

// private static members

METHOD_HANDLER_DEFINE(helix_dbus_server_handle_open_uri);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_play);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_pause);

static HelixDbusMethodVTable method_handler_vtable [] = {
    { "OpenUri", DBUS_TYPE_BOOLEAN, helix_dbus_server_handle_open_uri },
    { "Play",    DBUS_TYPE_INVALID, helix_dbus_server_handle_play },
    { "Pause",   DBUS_TYPE_INVALID, helix_dbus_server_handle_pause },
    { NULL,      0,              NULL }
};

static DBusHandlerResult helix_dbus_server_path_message(DBusConnection *connection, 
    DBusMessage *message, void *user_data);

static DBusObjectPathVTable dbus_call_vtable = {
    NULL, helix_dbus_server_path_message, NULL
};

static HelixDbusMethodVTable * 
helix_dbus_server_method_find(const gchar *method)
{
    gint i;
    
    for(i = 0; method_handler_vtable[i].method_name != NULL; i++) {
        if(g_strcasecmp(method_handler_vtable[i].method_name, method) == 0) {
            return &method_handler_vtable[i];
        }
    }
    
    return NULL;
}

static DBusHandlerResult 
helix_dbus_server_path_message(DBusConnection *connection, DBusMessage *message, void *user_data)
{
    HelixDbusServer *server = (HelixDbusServer *)user_data;
    DBusMessage *reply;
    HelixDbusMethodVTable *method;
    gpointer method_return;
    
    if(dbus_message_get_type(message) != DBUS_MESSAGE_TYPE_METHOD_CALL) {
        return DBUS_HANDLER_RESULT_NOT_YET_HANDLED;
    }
    
    method = helix_dbus_server_method_find(dbus_message_get_member(message));
    if(method == NULL) {
        return DBUS_HANDLER_RESULT_NOT_YET_HANDLED;
    }

    method->handler(server, message, &method_return);
    
    reply = dbus_message_new_method_return(message);
    if(method->return_type != DBUS_TYPE_INVALID && method_return != NULL) {
        dbus_message_append_args(reply, method->return_type, &method_return, DBUS_TYPE_INVALID);
    }
    
    dbus_connection_send(connection, reply, 0);
    dbus_connection_flush(connection);
    
    return DBUS_HANDLER_RESULT_HANDLED;
}

static DBusConnection *
helix_dbus_server_connect_to_dbus(gpointer user_data)
{
    DBusConnection *connection;
    DBusError error;
    
    dbus_error_init(&error);
    connection = dbus_bus_get(DBUS_BUS_SESSION, &error);

    if(connection == NULL || dbus_error_is_set(&error)) {
        g_warning("Unable to connect to dbus: %s", error.message);
        dbus_error_free(&error);
        return NULL;
    }
    
    if(!dbus_connection_register_object_path(connection, HELIX_DBUS_PLAYER_PATH, 
        &dbus_call_vtable, user_data)) {
        g_warning("Unable to register object path " HELIX_DBUS_PLAYER_PATH);
        dbus_connection_close(connection);
        dbus_connection_unref(connection);
        return NULL;
    }

    if(dbus_bus_request_name(connection, HELIX_DBUS_SERVICE, 0, NULL) == -1) {
        g_warning("Unable to request service name " HELIX_DBUS_SERVICE);
        dbus_connection_close(connection);
        dbus_connection_unref(connection);
        return NULL;
    }
    
    return connection;
}

// private dbus methods

static void 
helix_dbus_server_handle_open_uri(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    const gchar *uri;
    
    if(dbus_message_get_args(message, NULL, DBUS_TYPE_STRING, &uri, DBUS_TYPE_INVALID)) {
        *return_value = (gpointer)hxplayer_open(server->player, uri);
    }
}

static void 
helix_dbus_server_handle_play(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    hxplayer_play(server->player);
}

static void 
helix_dbus_server_handle_pause(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    hxplayer_pause(server->player);
}

static int
hxmessage_segment_type_to_dbus_type(HxMessageSegmentType segment_type)
{
    switch(segment_type) {
        case HX_MESSAGE_SEGMENT_INT32: return DBUS_TYPE_INT32;
        case HX_MESSAGE_SEGMENT_UINT32: return DBUS_TYPE_UINT32;
        case HX_MESSAGE_SEGMENT_STRING: return DBUS_TYPE_STRING;
        default: return DBUS_TYPE_INVALID;
    }
}

static void
helix_dbus_server_hxplayer_message_handler(HxPlayer *player, HxMessage *message)
{
    HelixDbusServer *server = (HelixDbusServer *)hxplayer_get_user_info(player);
    DBusMessage *signal;
    HxMessageType message_type;
    GList *message_segments;
    int segment_index, segment_count;
    
    if(message == NULL) {
        return;
    }
    
    message_type = hxmessage_get_type(message);
    if(message_type == HX_MESSAGE_NONE) {
        return;
    }
    
    signal = dbus_message_new_signal(HELIX_DBUS_PLAYER_PATH, HELIX_DBUS_INTERFACE, "Message");
    dbus_message_append_args(signal, DBUS_TYPE_INT32, &message_type, DBUS_TYPE_INVALID);
    
    message_segments = hxmessage_get_segment_list(message);
    if(message_segments != NULL) {
        for(segment_index = 0, segment_count = g_list_length(message_segments); 
            segment_index < segment_count; segment_index++) {
            
            HxMessageSegment *message_segment;
            HxMessageSegmentType segment_type;
            const gchar *segment_name;
            gpointer segment_value;
            
            message_segment = (HxMessageSegment *)g_list_nth_data(message_segments, segment_index);
            if(message_segment == NULL) {
                break;
            }
            
            segment_type = hxmessage_segment_get_type(message_segment);
            segment_name = hxmessage_segment_get_name(message_segment);
            segment_value = hxmessage_segment_get_value(message_segment);
            
            dbus_message_append_args(signal, 
                DBUS_TYPE_STRING, &segment_name,
                hxmessage_segment_type_to_dbus_type(segment_type), &segment_value, 
                DBUS_TYPE_INVALID);
        }
    }
    
    hxmessage_free(message);
    
    dbus_connection_send(server->connection, signal, NULL);
    dbus_connection_flush(server->connection);
    dbus_message_unref(signal);
}

// public methods

HelixDbusServer *
helix_dbus_server_new()
{
    HelixDbusServer *server = g_new0(HelixDbusServer, 1);
 
    if(server == NULL) {
        return NULL;
    }
    
    server->player = hxplayer_new();
    if(server->player == NULL) {
        g_free(server);
        return NULL;
    }
    
    server->connection = helix_dbus_server_connect_to_dbus(server);
    if(server->connection == NULL) {
        g_free(server);
        return NULL;
    }
    
    hxplayer_set_user_info(server->player, server);
    hxplayer_set_message_callback(server->player, helix_dbus_server_hxplayer_message_handler);
    hxplayer_pump_begin(server->player);
    
    return server;
}
    
void
helix_dbus_server_free(HelixDbusServer *server)
{
    if(server == NULL) {
        return;
    }
    
    if(server->connection != NULL) {
        dbus_connection_unregister_object_path(server->connection, HELIX_DBUS_PLAYER_PATH);
        
        if(dbus_connection_get_is_connected(server->connection)) {
            dbus_connection_close(server->connection);
        }
    
        dbus_connection_unref(server->connection);
        server->connection = NULL;
    }
    
    hxplayer_pump_end(server->player);
    
    if(server->player != NULL) {
        hxplayer_free(server->player);
        server->player = NULL;
    }
    
    g_free(server);
    server = NULL;
}

DBusConnection *
helix_dbus_server_get_dbus_connection(HelixDbusServer *server)
{
    if(server == NULL) {
        return NULL;
    }
    
    return server->connection;
}

