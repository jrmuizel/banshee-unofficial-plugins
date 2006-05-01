AC_INIT(README)
AC_CANONICAL_SYSTEM

AM_INIT_AUTOMAKE(banshee-wikipedia-plugin, 0.0.2)

AM_MAINTAINER_MODE

AC_PATH_PROG(MONO, mono, no)
if test "x$MONO" = "xno"; then
	AC_MSG_ERROR([Cannot find "mono" runtime in your PATH])
fi

AC_PATH_PROG(MCS, mcs, no)
if test "x$MCS" = "xno"; then
	AC_MSG_ERROR([Cannot find "mcs" compiler in your PATH])
fi

PKG_CHECK_MODULES(BANSHEE, banshee >= 0.10.2)
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

