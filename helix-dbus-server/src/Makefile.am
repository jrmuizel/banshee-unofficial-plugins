INCLUDES = \
	-Wall \
	-Werror \
	-D_FORTIFY_SOURCE=2 \
	-ggdb3 \
	--permissive \
	-I$(top_srcdir)/src/helix/player/hxclientkit/pub \
	-I$(top_srcdir)/src/helix/common/runtime/pub \
	-D_UNIX \
	$(GNOME_CFLAGS) \
	-DDBUS_API_SUBJECT_TO_CHANGE

helixdbusdir = $(bindir)
helixdbus_PROGRAMS = helix-dbus-server.bin
helixdbus_SCRIPTS = helix-dbus-server

helix_EXTRAS = \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientCallbacks.h \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientCFuncs.h \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientConstants.h \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientCProcPtrs.h \
	$(top_srcdir)/src/helix/player/hxclientkit/pub/HXClientTypes.h \
	$(top_srcdir)/src/helix/player/hxclientkit/dbg/libhxclient.a

helix_dbus_server_bin_SOURCES = \
	$(srcdir)/hxplayer.cc \
	$(srcdir)/hxplayer.h \
	$(srcdir)/hxmessage.cc \
	$(srcdir)/hxmessage.h \
	$(srcdir)/hxmessagesegment.cc \
	$(srcdir)/hxmessagesegment.h \
	$(srcdir)/helix-dbus-server.cc \
	$(srcdir)/helix-dbus-server.h \
	$(srcdir)/helix-dbus-server-main.cc

helix_dbus_server_bin_LDADD = $(top_srcdir)/src/helix/player/hxclientkit/dbg/libhxclient.a
helix_dbus_server_bin_LDFLAGS = -L$(top_srcdir)/src/helix/player/hxclientkit/dbg -lpthread -ldl -lhxclient $(GNOME_LIBS)

CLEANFILES = 
MAINTAINERCLEANFILES = Makefile.in
EXTRA_DIST = $(helix_dbus_server_SOURCES) $(helix_EXTRAS)

