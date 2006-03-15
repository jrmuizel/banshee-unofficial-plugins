libgstxingencdir = $(GST_PLUGINS_DIR)
libgstxingenc_LTLIBRARIES = libgstxingenc.la

INCLUDES = -D_FORTIFY_SOURCE=2 -fPIC -DPIC $(GSTREAMER_CFLAGS)

libgstxingenc_la_LDFLAGS = -avoid-version
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
	gst-xing-encoder.c

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
	gst-xing-encoder.h

MAINTAINERCLEANFILES = Makefile.in

