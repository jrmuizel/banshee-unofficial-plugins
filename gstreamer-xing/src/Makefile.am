libgstxingencdir = $(DESTDIR)$(GST_PLUGINS_DIR)
libgstxingenc_LTLIBRARIES = libgstxingenc.la

INCLUDES = -D_FORTIFY_SOURCE=2 -fPIC -DPIC $(GSTREAMER_CFLAGS)

if HAVE_GST_10
GST_XING_PLUGIN_SOURCE = gst-xing-encoder-0.10.c
else
GST_XING_PLUGIN_SOURCE = gst-xing-encoder-0.8.c
endif

libgstxingenc_la_LDFLAGS = $(GST_PLUGIN_LDFLAGS)
libgstxingenc_la_SOURCES = \
	amodini2.c \
	cnt.c \
	cnts.c \
	detect.c \
	emap.c \
	emdct.c \
	filter2.c \
	hwin.c \
	l3init.c \
	l3math.c \
	l3pack.c \
	mhead.c \
	pcmhpm.c \
	pow34.c \
	sbt.c \
	setup.c \
	spdsmr.c \
	xhead.c \
	xhwin.c \
	xsbt.c \
	bitallo1.cpp \
	bitallo3.cpp \
	bitalloc.cpp \
	bitallo.cpp \
	bitallosc.cpp \
	bitallos.cpp \
	mp3enc.cpp \
	srcc.cpp \
	srccfb.cpp \
	srccf.cpp \
	$(GST_XING_PLUGIN_SOURCE)

EXTRA_DIST = \
	balow.h \
	cnttab.h \
	htable.h \
	l3map.h \
	l3tcp.h \
	mhead.h \
	mp3low.h \
	tableaw2.h \
	tat.h \
	tcp.h \
	amod.h \
	mp3low.h \
	bitallo1.h \
	bitallo3.h \
	bitallo.h \
	bitallos.h \
	encapp.h \
	enc.h \
	filter2.h \
	hxtypes.h \
	l3e.h \
	mp3enc.h \
	pcmhpm.h \
	srcc.h \
	xhead.h \
	gst-xing-encoder.h \
	gst-xing-encoder-0.8.c \
	gst-xing-encoder-0.10.c

MAINTAINERCLEANFILES = Makefile.in

