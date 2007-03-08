AC_INIT(README)
AC_CANONICAL_SYSTEM

AM_INIT_AUTOMAKE(banshee-locker-plugin, 0.1)

AM_MAINTAINER_MODE

AC_PROG_INSTALL

MONO_REQUIRED_VERSION=1.1.13
PKG_CHECK_MODULES(MONO, mono >= $MONO_REQUIRED_VERSION)

dnl Mono programs
MONODIR=`$PKG_CONFIG --variable=exec_prefix mono`
MONOBIN="$MONODIR/bin"
MONO="$MONOBIN/mono"
MCS="$MONOBIN/mcs"
GMCS="$MONOBIN/gmcs"

AC_PATH_PROG(MONO, mono, no)
if test "x$MONO" = "xno"; then
	AC_MSG_ERROR([Cannot find "mono" runtime in your PATH])
fi

if test ! -x "$MCS"; then
	AC_MSG_ERROR([No $CS compiler found])
fi

if test ! -x "$GMCS"; then
	AC_MSG_ERROR([No $CS 2.0 (gmcs) compiler found])
fi

if test "x`$PKG_CONFIG --variable=pluginapi banshee`" = "x2"; then
	banshee_0_10="no"
	MCS=$GMCS
	AM_CONDITIONAL(BANSHEE_0_10, false)
else
	banshee_0_10="yes"
	MCS_FLAGS="$MCS_FLAGS -define:BANSHEE_0_10"
	AM_CONDITIONAL(BANSHEE_0_10, true)
fi

AC_SUBST(MCS)

PKG_CHECK_MODULES(BANSHEE, banshee >= 0.10.4)
AC_SUBST(BANSHEE_LIBS)

AC_ARG_ENABLE(user-plugin, AC_HELP_STRING([--enable-user-plugin], [Install plugin into user-level Banshee plugins directory instead of the Banshee system plugins directory]), , enable_user_plugin="no")

PLUGINDIR=`$PKG_CONFIG --variable=systemplugindir banshee`
if test "x$enable_user_plugin" = "xyes"; then
	PLUGINDIR=`$PKG_CONFIG --variable=userplugindir banshee`
	PLUGINDIR=`echo "$PLUGINDIR" | sed -e "s|\~|$HOME|"`
fi
AC_SUBST(PLUGINDIR)

AC_OUTPUT([
	Makefile
	src/Makefile
])

echo "
Plugin will be installed in $PLUGINDIR
You should now run \`make'
";

