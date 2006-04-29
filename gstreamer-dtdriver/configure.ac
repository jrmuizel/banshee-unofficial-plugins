AC_PREREQ(2.52)

AC_INIT(README)
AM_CONFIG_HEADER(config.h)
AC_CONFIG_SRCDIR(src)
AM_INIT_AUTOMAKE(gstreamer-dtdriver, 0.10.1)

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

PKG_CHECK_MODULES(GSTREAMER,
	gstreamer-0.10 >= 0.10.3
	gstreamer-base-0.10 >= 0.10.3
	gstreamer-plugins-base-0.10 >= 0.10.3
)

AC_SUBST(GSTREAMER_CFLAGS)
AC_SUBST(GSTREAMER_LIBS)

GST_PLUGINS_DIR="`$PKG_CONFIG --variable=libdir gstreamer-${GST_VERSION}`/gstreamer-${GST_VERSION}"
if test ! -d "$GST_PLUGINS_DIR"; then
	AC_MSG_ERROR([GStreamer plugins directory could not be found])
fi
AC_SUBST(GST_PLUGINS_DIR)

GST_PLUGIN_LDFLAGS="-module -avoid-version -export-symbols-regex '^[_]*gst_plugin_desc\$\$' -no-undefined"
AC_SUBST(GST_PLUGIN_LDFLAGS)

dnl HELIX_LIBS stuff

AC_ARG_WITH(helix-libs, AC_HELP_STRING([--with-helix-libs], [Specify Helix/RealPlayer Library/Codecs Path]), helix_libs="$withval", helix_libs="no")

COMMON_HELIX_SIGNATURE="common/clntcore.so"
POSSIBLE_HELIX_LIBS="/usr/lib/RealPlayer10 /usr/lib/RealPlayer /usr/lib/realplayer /usr/local/realplayer /usr/local/RealPlayer10 /usr/local/RealPlayer /opt/realplayer /usr/lib/realplay-10.0.5 /usr/lib/realplay-10.0.4 /usr/lib/realplay-10.0.3 /usr/lib/realplay-10.0.2 /usr/lib/realplay-10.0.1 /usr/lib/realplay-10.0.0 /usr/lib/realplay-10.0 /usr/lib/helix /usr/local/Helix /usr/local/HelixPlayer /usr/lib/helix/player /usr/local/helix/player /usr/local/lib/helix/player"

if test "x$helix_libs" = "xno"; then
	if test ! -e $helix_libs/$COMMON_HELIX_SIGNATURE; then
		helix_libs="no";
		for possible in $POSSIBLE_HELIX_LIBS; do
			if test -e $possible/$COMMON_HELIX_SIGNATURE ; then
				helix_libs="$possible";
				break;
			fi
		done
	fi
else
	if test ! -e $helix_libs/$COMMON_HELIX_SIGNATURE; then
		helix_libs="no";
	fi
fi

if test "x$helix_libs" = "xno"; then
	AC_MSG_ERROR([A RealPlayer/Helix install could not be found. Please configure with --with-helix-libs=/path/to/install])
else
	HELIX_LIBRARY_PATH="$helix_libs";
	HELIX_PLUGINS_PATH="$helix_libs/plugins";
	HELIX_CODECS_PATH="$helix_libs/codecs";
	AC_SUBST(HELIX_LIBRARY_PATH)
	AC_SUBST(HELIX_PLUGINS_PATH)
	AC_SUBST(HELIX_CODECS_PATH)
	AC_DEFINE_UNQUOTED(HELIX_LIBRARY_PATH, ["$HELIX_LIBRARY_PATH"], [RealPlayer/Helix Library Location])
	AC_DEFINE_UNQUOTED(HELIX_PLUGINS_PATH, ["$HELIX_PLUGINS_PATH"], [RealPlayer/Helix Plugins Location])
	AC_DEFINE_UNQUOTED(HELIX_CODECS_PATH, ["$HELIX_CODECS_PATH"], [RealPlayer/Helix Codecs Location])
fi

AC_OUTPUT([
Makefile
src/Makefile
])

