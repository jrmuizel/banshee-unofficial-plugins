AC_PREREQ(2.52)

AC_INIT(README)
AM_CONFIG_HEADER(config.h)
AC_CONFIG_SRCDIR(src)
AM_INIT_AUTOMAKE(helix-dbus-server, 0.2.3)

AM_MAINTAINER_MODE
AC_PROG_CC
AC_PROG_CXX
AC_ISC_POSIX
AC_HEADER_STDC
AM_PROG_LIBTOOL

AS_AC_EXPAND(DATADIR, $datadir)

DBUS_SERVICES_DIR="$DATADIR/dbus-1/services"
AC_SUBST(DBUS_SERVICES_DIR)
AC_DEFINE_UNQUOTED(DBUS_SERVICES_DIR, "$DBUS_SERVICES_DIR", [Where services dir for DBUS is])

GNOME_DEBUG_CHECK
GNOME_COMPILE_WARNINGS(error)
AM_PATH_GLIB_2_0

dnl Pkgconfig
PKG_CHECK_MODULES(GNOME,
	gobject-2.0 \
	glib-2.0 \
	dbus-1 \
	dbus-glib-1)
AC_SUBST(GNOME_CFLAGS)
AC_SUBST(GNOME_LIBS)

dnl Mono (for client sample)

AC_PATH_PROG(MONO, mono, no)
AC_PATH_PROG(MCS, mcs, no)
AM_CONDITIONAL(HAVE_MONO, false)

if test ! "x$MONO" = "xno"; then
	if test ! "x$MCS" = "xno"; then
		PKG_CHECK_MODULES(DBUS_SHARP, dbus-sharp >= 0.60)
		AC_SUBST(DBUS_SHARP_LIBS)
		AM_CONDITIONAL(HAVE_MONO, true)
		AC_SUBST(MONO)
		AC_SUBST(MCS)
	fi
fi


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
	AC_SUBST(HELIX_LIBRARY_PATH)
	AC_DEFINE_UNQUOTED(HELIX_LIBRARY_PATH, ["$HELIX_LIBRARY_PATH"], [RealPlayer/Helix Library Location])
fi

expanded_bindir=`( case $prefix in NONE) prefix=$ac_default_prefix ;; *) ;; esac
		case $exec_prefix in NONE) exec_prefix=$prefix ;; *) ;; esac
		eval echo $bindir )`
AC_SUBST(expanded_bindir)

AC_OUTPUT([
Makefile
src/Makefile
src/helix-dbus-server
data/Makefile
data/helix-dbus-server.pc
])

echo "
    Helix Libraries: ${HELIX_LIBRARY_PATH}
"
