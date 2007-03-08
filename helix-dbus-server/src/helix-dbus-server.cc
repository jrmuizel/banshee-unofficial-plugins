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
#include <string.h>
#include <time.h>
#include <glib.h>
#include <glib/gstdio.h>
#include <dbus/dbus.h>
#include <dbus/dbus-glib.h>

#include "HXClientCFuncs.h"

#include "hxplayer.h"
#include "hxmessage.h"
#include "hxmessagesegment.h"

#include "helix-dbus-server.h"

// private definitions

struct HelixDbusServer {
    DBusConnection *connection;
    HelixDbusServerShutdownCallback shutdown_cb;
    HxPlayer *player;
    time_t last_ping;
    guint ping_timeout;
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

#define PLAYER_TOKEN(player) hxplayer_get_player_token(player)

// private static members

METHOD_HANDLER_DEFINE(helix_dbus_server_handle_ping);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_open_uri);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_play);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_pause);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_stop);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_start_seeking);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_stop_seeking);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_get_group_title);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_get_volume);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_set_volume);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_get_length);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_get_position);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_set_position);
METHOD_HANDLER_DEFINE(helix_dbus_server_handle_get_is_live);

static HelixDbusMethodVTable method_handler_vtable [] = {
    { "Ping",            DBUS_TYPE_INVALID,   helix_dbus_server_handle_ping },
    { "OpenUri",         DBUS_TYPE_BOOLEAN,   helix_dbus_server_handle_open_uri },
    { "Play",            DBUS_TYPE_INVALID,   helix_dbus_server_handle_play },
    { "Pause",           DBUS_TYPE_INVALID,   helix_dbus_server_handle_pause },
    { "Stop",            DBUS_TYPE_INVALID,   helix_dbus_server_handle_stop },
    { "StartSeeking",    DBUS_TYPE_BOOLEAN,   helix_dbus_server_handle_start_seeking },
    { "StopSeeking",     DBUS_TYPE_INVALID,   helix_dbus_server_handle_stop_seeking },
    { "GetGroupTitle",   DBUS_TYPE_STRING,    helix_dbus_server_handle_get_group_title },
    { "GetVolume",       DBUS_TYPE_UINT32,    helix_dbus_server_handle_get_volume },
    { "SetVolume",       DBUS_TYPE_UINT32,    helix_dbus_server_handle_set_volume },
    { "GetLength",       DBUS_TYPE_UINT32,    helix_dbus_server_handle_get_length },
    { "GetPosition",     DBUS_TYPE_UINT32,    helix_dbus_server_handle_get_position },
    { "SetPosition",     DBUS_TYPE_BOOLEAN,   helix_dbus_server_handle_set_position },
    { "GetIsLive",       DBUS_TYPE_BOOLEAN,   helix_dbus_server_handle_get_is_live },
    { NULL,              0,                   NULL }
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
    const gchar *method_name;
    
    if(dbus_message_get_type(message) != DBUS_MESSAGE_TYPE_METHOD_CALL) {
        return DBUS_HANDLER_RESULT_NOT_YET_HANDLED;
    }
    
    method_name = dbus_message_get_member(message);
    if(strcmp(method_name, "Shutdown") == 0) {
        reply = dbus_message_new_method_return(message);
        dbus_connection_send(connection, reply, 0);
        dbus_connection_flush(connection);
        dbus_message_unref(reply);
        
        server->shutdown_cb(server);
        
        return DBUS_HANDLER_RESULT_HANDLED;
    }
        
    method = helix_dbus_server_method_find(method_name);
    if(method == NULL) {
        return DBUS_HANDLER_RESULT_NOT_YET_HANDLED;
    }

    method->handler(server, message, &method_return);
    
    if(method->return_type == DBUS_TYPE_STRING && method_return == NULL) {
        method_return = (void *)"";
    }
    
    reply = dbus_message_new_method_return(message);
    if(method->return_type != DBUS_TYPE_INVALID) {
        dbus_message_append_args(reply, method->return_type, &method_return, DBUS_TYPE_INVALID);
    }
    
    dbus_connection_send(connection, reply, 0);
    dbus_connection_flush(connection);
    dbus_message_unref(reply);
    
    return DBUS_HANDLER_RESULT_HANDLED;
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
            
            if(segment_type == HX_MESSAGE_SEGMENT_STRING && segment_value == NULL) {
                continue;
            }
            
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

static DBusConnection *
helix_dbus_server_connect_to_dbus(DBusConnection *connection, gpointer user_data)
{
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

static gboolean
helix_dbus_server_check_last_ping(gpointer user_data)
{
    HelixDbusServer *server = (HelixDbusServer *)user_data;

    if(server == NULL) {
        return FALSE;
    } else if(server->last_ping == 0 || (time(NULL) - server->last_ping) <= 10) {
        return TRUE;
    }
    
    server->ping_timeout = 0;
    server->shutdown_cb(server);
    return FALSE;
}

// private dbus methods

static void 
helix_dbus_server_handle_ping(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    server->last_ping = time(NULL);
}

static void 
helix_dbus_server_handle_open_uri(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    const gchar *uri;
    
    if(dbus_message_get_args(message, NULL, DBUS_TYPE_STRING, &uri, DBUS_TYPE_INVALID)) {
        *return_value = (gpointer)ClientPlayerOpenURL(PLAYER_TOKEN(server->player), uri, NULL);
    }
}

static void 
helix_dbus_server_handle_play(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    ClientPlayerPlay(PLAYER_TOKEN(server->player));
}

static void 
helix_dbus_server_handle_pause(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    ClientPlayerPause(PLAYER_TOKEN(server->player));
}

static void 
helix_dbus_server_handle_stop(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    ClientPlayerStop(PLAYER_TOKEN(server->player));
}

static void 
helix_dbus_server_handle_start_seeking(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    *return_value = (gpointer)ClientPlayerStartSeeking(PLAYER_TOKEN(server->player));
}

static void 
helix_dbus_server_handle_stop_seeking(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    ClientPlayerStopSeeking(PLAYER_TOKEN(server->player));
}

static void 
helix_dbus_server_handle_get_volume(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    *return_value = (gpointer)ClientPlayerGetVolume(PLAYER_TOKEN(server->player));
}

static void 
helix_dbus_server_handle_set_volume(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    guint volume = 0;
    
    if(dbus_message_get_args(message, NULL, DBUS_TYPE_UINT32, &volume, DBUS_TYPE_INVALID)) {
        ClientPlayerSetVolume(PLAYER_TOKEN(server->player), (gushort)volume);
    }
}

static void 
helix_dbus_server_handle_get_length(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    *return_value = (gpointer)ClientPlayerGetLength(PLAYER_TOKEN(server->player));
}

static void 
helix_dbus_server_handle_get_position(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    *return_value = (gpointer)ClientPlayerGetPosition(PLAYER_TOKEN(server->player));
}

static void 
helix_dbus_server_handle_set_position(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{    
    guint position = 0;
    *return_value = 0;
    
    if(dbus_message_get_args(message, NULL, DBUS_TYPE_UINT32, &position, DBUS_TYPE_INVALID)) {
        *return_value = (gpointer)ClientPlayerSetPosition(PLAYER_TOKEN(server->player), position);
    }
}

static void
helix_dbus_server_handle_get_group_title(HelixDbusServer *server, DBusMessage *message,
    void **return_value)
{
    guint group_index = 0;
    gchar buffer[256];
    UInt32 buffer_used_length = 0;
    
    *return_value = NULL;
    
    if(dbus_message_get_args(message, NULL, DBUS_TYPE_UINT32, &group_index, DBUS_TYPE_INVALID)) {
        if(ClientPlayerGetGroupTitle(PLAYER_TOKEN(server->player), (gushort)group_index, 
            buffer, sizeof(buffer), &buffer_used_length)) {
            *return_value = g_strdup(buffer);
        }
    }
}

static void 
helix_dbus_server_handle_get_is_live(HelixDbusServer *server, DBusMessage *message, 
    void **return_value)
{
    *return_value = (gpointer)ClientPlayerIsLive(PLAYER_TOKEN(server->player));
}


// public methods

HelixDbusServer *
helix_dbus_server_new(DBusConnection *connection, HelixDbusServerShutdownCallback shutdown_cb)
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
    
    server->connection = helix_dbus_server_connect_to_dbus(connection, server);
    if(server->connection == NULL) {
        g_free(server);
        return NULL;
    }
    
    hxplayer_set_user_info(server->player, server);
    hxplayer_set_message_callback(server->player, helix_dbus_server_hxplayer_message_handler);
    hxplayer_pump_begin(server->player);
    
    server->shutdown_cb = shutdown_cb;
    server->last_ping = 0;
    
    server->ping_timeout = g_timeout_add(5000, helix_dbus_server_check_last_ping, server);
    
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
    
    if(server->ping_timeout != 0) {
        g_source_remove(server->ping_timeout);
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

