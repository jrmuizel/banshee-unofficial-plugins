AC_INIT(README)
AC_CANONICAL_SYSTEM

AM_INIT_AUTOMAKE(banshee-engine-xine, 0.1)

AM_MAINTAINER_MODE

AC_PATH_PROG(MONO, mono, no)
if test "x$MONO" = "xno"; then
	AC_MSG_ERROR([Cannot find "mono" runtime in your PATH])
fi

AC_PATH_PROG(MCS, gmcs, no)
if test "x$MCS" = "xno"; then
	AC_MSG_ERROR([Cannot find "gmcs" compiler in your PATH])
fi

PKG_CHECK_MODULES(GLIB_SHARP, glib-sharp-2.0)
PKG_CHECK_MODULES(BANSHEE, banshee >= 0.10.10)
PKG_CHECK_MODULES(LIBXINE, libxine)
AC_SUBST(BANSHEE_LIBS)

PLUGINS_DIR="`$PKG_CONFIG --variable=systemplugindir banshee`"
BANSHEE_ENGINE_DIR="`echo $PLUGINS_DIR | sed -e 's/Banshee.Plugins//'`Banshee.MediaEngine/Xine"

AC_SUBST(BANSHEE_ENGINE_DIR)

AC_OUTPUT([
	Makefile
	src/Makefile
])

