INCLUDES = -Wall -D_FORTIFY_SOURCE=2 -ggdb3 --permissive -I$(top_srcdir)/src/helix/player/hxclientkit/pub -I$(top_srcdir)/src/helix/common/runtime/pub -D_UNIX $(GNOME_CFLAGS) -DDBUS_API_SUBJECT_TO_CHANGE

helixdbusdir = $(bindir)
helixdbus_PROGRAMS = helix-dbus-server 

helix_dbus_server_SOURCES = \
	hxplayer.cc \
	hxplayer.h \
	hxmessage.cc \
	hxmessage.h \
	hxmessagesegment.cc \
	hxmessagesegment.h \
	helix-dbus-server.cc \
	helix-dbus-server.h \
	helix-dbus-player.cc

helix_dbus_server_LDADD = libhxclient.a
helix_dbus_server_LDFLAGS = -L. -lpthread -ldl -lhxclient $(GNOME_LIBS)

CLEANFILES = 
MAINTAINERCLEANFILES = Makefile.in

if HAVE_MONO

ASSEMBLY = HelixDbusClient.exe
CLIENT_SRC = HelixDbusClient.cs

all: $(ASSEMBLY)

$(ASSEMBLY): $(CLIENT_SRC)
	$(MCS) -out:$@ $(DBUS_SHARP_LIBS) -pkg:gtk-sharp-2.0 $(CLIENT_SRC)

CLEANFILES += $(ASSEMBLY)

endif

EXTRA_DIST = HelixDbusClient.cs

server-run:
	HELIX_LIBS=/usr/lib/RealPlayer10/ ./helix-dbus-server

client-run:
	mono --debug HelixDbusClient.exe

