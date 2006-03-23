/***************************************************************************
 *  helix-dbus-server.h
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

#ifndef _HELIX_DBUS_SERVER_H
#define _HELIX_DBUS_SERVER_H

#include <dbus/dbus.h>

#define HELIX_DBUS_SERVICE "org.gnome.HelixDbusPlayer"
#define HELIX_DBUS_INTERFACE "org.gnome.HelixDbusPlayer"
#define HELIX_DBUS_PLAYER_PATH "/org/gnome/HelixDbusPlayer/Player"

typedef struct HelixDbusServer HelixDbusServer;

typedef void (* HelixDbusServerShutdownCallback) (HelixDbusServer *server);

HelixDbusServer *helix_dbus_server_new(DBusConnection *connection, HelixDbusServerShutdownCallback shutdown_cb);
void helix_dbus_server_free(HelixDbusServer *server);

DBusConnection *helix_dbus_server_get_dbus_connection(HelixDbusServer *server);

#endif /* _HELIX_DBUS_SERVER_H */
