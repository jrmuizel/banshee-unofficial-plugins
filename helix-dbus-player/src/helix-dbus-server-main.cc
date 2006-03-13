/***************************************************************************
 *  helix-dbus-server-main.cc
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

#ifdef HAVE_CONFIG_H
#include "config.h"
#endif

#include <stdlib.h>
#include <stdio.h>
#include <glib.h>
#include <glib-object.h>
 
#include <dbus/dbus-glib-lowlevel.h>
 
#include "helix-dbus-server.h"
 
static GMainLoop *loop = NULL;
 
static void
on_helix_dbus_server_shutdown(HelixDbusServer *server)
{
    g_main_loop_quit(loop);
}
 
gint 
main(gint argc, gchar **argv)
{
    DBusConnection *connection;
    DBusError error;
    HelixDbusServer *server;
    
    g_type_init();
    
    dbus_error_init(&error);
    connection = dbus_bus_get(DBUS_BUS_SESSION, &error);

    if(connection == NULL || dbus_error_is_set(&error)) {
        g_error("Unable to connect to dbus: %s", error.message);
        dbus_error_free(&error);
        exit(1);
    }
 
    if(dbus_bus_name_has_owner(connection, HELIX_DBUS_SERVICE, NULL)) {
        g_warning("helix-dbus-server is already running");
        dbus_connection_close(connection);
        exit(1);
    }
    
    setenv("HELIX_LIBS", HELIX_LIBRARY_PATH, 0);

    server = helix_dbus_server_new(connection, on_helix_dbus_server_shutdown);
    
    if(server == NULL) {
        g_error("Could not create Helix DBus Server. Configured with HELIX_LIBS=" 
            HELIX_LIBRARY_PATH ". Current HELIX_LIBS=%s. Try manually setting HELIX_LIBS?",
            getenv("HELIX_LIBS"));
        exit(1);
    }
    
    loop = g_main_loop_new(NULL, FALSE);
    dbus_connection_setup_with_g_main(connection, g_main_loop_get_context(loop));
    
    g_main_loop_run(loop);

    exit(0);
}
