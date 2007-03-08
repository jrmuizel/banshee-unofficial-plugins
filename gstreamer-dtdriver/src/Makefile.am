libgstdtdriverdir = $(GST_PLUGINS_DIR)
libgstdtdriver_LTLIBRARIES = libgstdtdriver.la

INCLUDES = -Wall -ggdb3 -D_FORTIFY_SOURCE=2 -fPIC -DPIC $(GSTREAMER_CFLAGS) -I$(top_srcdir)/helix/hxfftrans/pub 

libgstdtdriver_la_LDFLAGS = $(GST_PLUGIN_LDFLAGS) -L$(top_srcdir)/helix/hxfftrans/dbg -lhxfftrans -lpthread -ldl -lm -lstdc++
libgstdtdriver_la_LIBADD = $(GSTREAMER_LIBS) $(top_srcdir)/helix/hxfftrans/dbg/libhxfftrans.a
libgstdtdriver_la_SOURCES = \
	gstdtdriver.c \
	gstdtdriver.h

noinst_PROGRAMS = test
test_SOURCES = test.c
test_LDADD = $(GSTREAMER_LIBS)

EXTRA_DIST = $(libgstdtdriver_la_SOURCES) $(test_SOURCES) 

MAINTAINERCLEANFILES = Makefile.in

