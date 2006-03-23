INCLUDES = -Wall -D_FORTIFY_SOURCE=2 -ggdb3 --permissive -I$(top_srcdir)/src/helix/player/hxclientkit/pub -I$(top_srcdir)/src/helix/common/runtime/pub -D_UNIX $(GNOME_CFLAGS) -DDBUS_API_SUBJECT_TO_CHANGE

helixdbusdir = $(bindir)
helixdbus_PROGRAMS = helix-dbus-server 

helix_EXTRAS = \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientCallbacks.h \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientCFuncs.h \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientConstants.h \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientCProcPtrs.h \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientTypes.h \
	$(top_srcdir)/src/helix/player/hxclientkit/dbg/libhxclient.a

helix_dbus_server_SOURCES = \
	$(srcdir)/hxplayer.cc \
	$(srcdir)/hxplayer.h \
	$(srcdir)/hxmessage.cc \
	$(srcdir)/hxmessage.h \
	$(srcdir)/hxmessagesegment.cc \
	$(srcdir)/hxmessagesegment.h \
	$(srcdir)/helix-dbus-server.cc \
	$(srcdir)/helix-dbus-server.h \
	$(srcdir)/helix-dbus-server-main.cc

helix_dbus_server_LDADD = $(top_srcdir)/src/helix/player/hxclientkit/dbg/libhxclient.a
helix_dbus_server_LDFLAGS = -L$(top_srcdir)/src/helix/player/hxclientkit/dbg -lpthread -ldl -lhxclient $(GNOME_LIBS)

CLEANFILES = 
MAINTAINERCLEANFILES = Makefile.in

EXTRA_DIST = $(helix_dbus_server_SOURCES) $(helix_EXTRAS) $(srcdir)/HelixRemoteClient.cs $(srcdir)/HelixRemotePlayer.cs

if HAVE_MONO

ASSEMBLY = HelixDbusClient.exe
CLIENT_SRC = $(srcdir)/HelixRemoteClient.cs $(srcdir)/HelixRemotePlayer.cs

all: $(ASSEMBLY)

$(ASSEMBLY): $(CLIENT_SRC)
	$(MCS) -out:$@ $(DBUS_SHARP_LIBS) -pkg:gtk-sharp-2.0 $(CLIENT_SRC)

CLEANFILES += $(ASSEMBLY)

endif

server-run:
	HELIX_LIBS=/usr/lib/RealPlayer10/ ./helix-dbus-server

client-run:
	mono --debug HelixDbusClient.exe

