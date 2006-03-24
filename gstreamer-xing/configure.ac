AC_PREREQ(2.52)

AC_INIT(README)
AM_CONFIG_HEADER(config.h)
AC_CONFIG_SRCDIR(src)
AM_INIT_AUTOMAKE(gstreamer-xing, 0.10.0)

AM_MAINTAINER_MODE
AC_PROG_CC
AC_PROG_CXX
AC_ISC_POSIX
AC_HEADER_STDC
AM_PROG_LIBTOOL

GNOME_DEBUG_CHECK
GNOME_COMPILE_WARNINGS(error)
AM_PATH_GLIB_2_0

GST_VERSION=0.10

AC_ARG_ENABLE(gstreamer, AC_HELP_STRING([--enable-gstreamer], [Enable GStreamer 0.8 or 0.10]), 
	[case "${enableval}" in
		yes)  GST_VERSION=0.10 ;;
		0.8)  GST_VERSION=0.8  ;;
		0.10) GST_VERSION=0.10 ;;
		*) AC_MSG_ERROR([Bad value ${enableval} for --enable-gstreamer])
	esac],
	[GST_VERSION=0.10])

if test "x$GST_VERSION" = "x0.8"; then
	PKG_CHECK_MODULES(GSTREAMER,
		gstreamer-0.8 >= 0.8.11
		gstreamer-plugins-0.8 >= 0.8.11
	)
else
	PKG_CHECK_MODULES(GSTREAMER,
		gstreamer-0.10 >= 0.10.3
		gstreamer-base-0.10 >= 0.10.3
		gstreamer-plugins-base-0.10 >= 0.10.3
	)
fi

AC_SUBST(GSTREAMER_CFLAGS)
AC_SUBST(GSTREAMER_LIBS)

GST_PLUGINS_DIR="`$PKG_CONFIG --variable=libdir gstreamer-${GST_VERSION}`/gstreamer-${GST_VERSION}"
if test ! -d "$GST_PLUGINS_DIR"; then
	AC_MSG_ERROR([GStreamer plugins directory could not be found])
fi
AC_SUBST(GST_PLUGINS_DIR)

AM_CONDITIONAL(HAVE_GST_10, test "x$GST_VERSION" = "x0.10")

GST_PLUGIN_LDFLAGS="-module -avoid-version -export-symbols-regex '^[_]*gst_plugin_desc\$\$' -no-undefined"
AC_SUBST(GST_PLUGIN_LDFLAGS)

AC_OUTPUT([
Makefile
src/Makefile
])

echo "
	GStreamer ${GST_VERSION}
"

