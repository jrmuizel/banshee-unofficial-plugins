/***************************************************************************
 *  helix-dbus-player.cc
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
#include <glib.h>
#include <glib-object.h>
 
#include <dbus/dbus-glib-lowlevel.h>
 
#include "helix-dbus-server.h"
 
static GMainLoop *loop = NULL;
static HelixDbusServer *server = NULL;
 
int main(int argc, char **argv)
{
    g_type_init();

    server = helix_dbus_server_new();
    
    if(server == NULL) {
        g_error("Could not create Helix DBus Server");
        exit(1);
    }
    
    loop = g_main_loop_new(NULL, FALSE);
    dbus_connection_setup_with_g_main(helix_dbus_server_get_dbus_connection(server), 
        g_main_loop_get_context(loop));
        
    g_main_loop_run(loop);

    exit(0);
}
