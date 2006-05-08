SUBDIRS = src

proxyfsysdir = $(HELIX_PLUGINS_PATH)
proxyfsys_SCRIPTS = $(top_srcdir)/proxyfsys/filesystem/local/dbg/proxyfsys.so

MAINTAINERCLEANFILES = \
	config.guess \
	config.h.in \
	config.sub \
	configure \
	depcomp \
	install-sh \
	ltmain.sh \
	Makefile.in \
	missing \
	aclocal.m4

helix_EXTRAS = \
	$(top_srcdir)/helix/README \
	$(top_srcdir)/helix/patches/dtdriver.diff \
	$(top_srcdir)/helix/patches/helix-gcc4.diff \
	$(top_srcdir)/helix/patches/proxyfsys.diff \
	$(top_srcdir)/helix/hxfftrans/pub/bbiosetup.h \
	$(top_srcdir)/helix/hxfftrans/pub/hxfftdriver.h \
	$(top_srcdir)/proxyfsys/filesystem/local/dbg/proxyfsys.so

EXTRA_DIST = $(helix_EXTRAS)

