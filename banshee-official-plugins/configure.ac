AC_INIT(README)

AC_CANONICAL_SYSTEM
AC_PREREQ(2.13)

VERSION=0.11.4
ASM_VERSION="$VERSION.*"

AM_INIT_AUTOMAKE(banshee-official-plugins, $VERSION)
AC_SUBST(ASM_VERSION)

AM_MAINTAINER_MODE

AC_PROG_INTLTOOL([0.21])

AC_PROG_INSTALL
AM_PROG_LIBTOOL

MONO_REQUIRED_VERSION=1.1.13
PKG_CHECK_MODULES(MONO, mono >= $MONO_REQUIRED_VERSION)

dnl Mono programs
MONODIR=`$PKG_CONFIG --variable=exec_prefix mono`
MONOBIN="$MONODIR/bin"
MONO="$MONOBIN/mono"
MCS="$MONOBIN/gmcs"

AC_MSG_CHECKING([for mono.pc])
if test -z `$PKG_CONFIG --variable=prefix mono`; then
	AC_MSG_ERROR([missing the mono.pc file, usually found in the mono-devel package])
else
	AC_MSG_RESULT([found])
fi

required_assemblies="Mono.Data.SqliteClient Mono.Posix System.Runtime.Remoting System.Web System.Web.Services"

for asm in $required_assemblies; do
	AC_MSG_CHECKING([for $asm.dll])
	if test ! -e `$PKG_CONFIG --variable=prefix mono`/lib/mono/2.0/$asm.dll; then
		AC_MSG_RESULT([not found])
		AC_MSG_ERROR([missing required Mono Assembly: $asm.dll])
	else
		AC_MSG_RESULT([found])
	fi
done

if test ! -x "$MONO"; then
	AC_MSG_ERROR([No Mono runtime found])
fi

CS="C#"
if test ! -x "$MCS"; then
	AC_MSG_ERROR([No $CS 2.0 (gmcs) compiler found])
fi

MCS_FLAGS="-debug -nowarn:0278"

AC_PATH_PROG(GCONFTOOL, gconftool-2, no)
AM_GCONF_SOURCE_2

dnl gtk-sharp, other CLR deps
GTKSHARP_REQUIRED=2.8
PKG_CHECK_MODULES(GTKSHARP,
	gtk-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gnome-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gnome-vfs-sharp-2.0 >= $GTKSHARP_REQUIRED \
	glade-sharp-2.0 >= $GTKSHARP_REQUIRED \
	gconf-sharp-2.0 >= $GTKSHARP_REQUIRED)
AC_SUBST(GTKSHARP_LIBS)

dnl Banshee deps
PKG_CHECK_MODULES(BANSHEE, banshee >= 0.11.4)
AC_SUBST(BANSHEE_LIBS)

AC_SUBST(MCS_FLAGS)
AC_SUBST(MCS)
AC_SUBST(MONO)

dnl Banshee Plugins Path

expanded_libdir=`( case $prefix in NONE) prefix=$ac_default_prefix ;; *) ;; esac
		   case $exec_prefix in NONE) exec_prefix=$prefix ;; *) ;; esac
		   eval echo $libdir )`
		   
pluginsdir=$libdir/banshee/Banshee.Plugins
expanded_pluginsdir=$expanded_libdir/banshee/Bashee.Plugins

AC_SUBST(expanded_libdir)
AC_SUBST(pluginsdir)

dnl Configure plugins to build

ENABLED_PLUGINS=""

AC_ARG_ENABLE(minimode, AC_HELP_STRING(--disable-minimode, [Do not build minimode plugin]), enable_minimode=no, enable_minimode=yes)
AM_CONDITIONAL(ENABLE_MINIMODE, test "x$enable_minimode" = "xyes")
if test "x$enable_minimode" = "xyes"; then 
	ENABLED_PLUGINS="$ENABLED_PLUGINS MiniMode"
fi

AC_ARG_ENABLE(radio, AC_HELP_STRING(--disable-radio, [Do not build radio plugin]), enable_radio=no, enable_radio=yes)
AM_CONDITIONAL(ENABLE_RADIO, test "x$enable_radio" = "xyes")
if test "x$enable_radio" = "xyes"; then 
	ENABLED_PLUGINS="$ENABLED_PLUGINS Radio"
fi

AC_ARG_ENABLE(recommendation, AC_HELP_STRING(--disable-recommendation, [Do not build recommendation plugin]), enable_recommendation=no, enable_recommendation=yes)
AM_CONDITIONAL(ENABLE_RECOMMENDATION, test "x$enable_recommendation" = "xyes")
if test "x$enable_recommendation" = "xyes"; then 
	ENABLED_PLUGINS="$ENABLED_PLUGINS Recommendation"
fi

AC_ARG_ENABLE(podcast, AC_HELP_STRING(--disable-podcast, [Do not build podcast plugin]), enable_podcast=no, enable_podcast=yes)
AM_CONDITIONAL(ENABLE_PODCAST, test "x$enable_podcast" = "xyes")
if test "x$enable_podcast" = "xyes"; then 
	ENABLED_PLUGINS="$ENABLED_PLUGINS Podcast"
fi


dnl Intl

ALL_LINGUAS="`cat "$srcdir/po/LINGUAS" | grep -v '^#'`"
GETTEXT_PACKAGE=banshee-plugins
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE, "$GETTEXT_PACKAGE", [Gettext Package])
AM_GLIB_GNU_GETTEXT
AC_SUBST([CONFIG_STATUS_DEPENDENCIES],['$(top_srcdir)/po/LINGUAS'])

dnl generated files

AC_OUTPUT([
Makefile
po/Makefile.in
build/Makefile
plugins/Makefile
plugins/Common.cs
plugins/MiniMode/Makefile
plugins/MiniMode/AssemblyInfo.cs
plugins/Podcast/Makefile
plugins/Podcast/AssemblyInfo.cs
plugins/Radio/Makefile
plugins/Radio/AssemblyInfo.cs
plugins/Recommendation/Makefile
plugins/Recommendation/AssemblyInfo.cs
])

echo "  
    Banshee Plugins Directory: ${expanded_pluginsdir}
    mcs flags: ${MCS_FLAGS}
    Enabled Plugins:
"

for plugin in $ENABLED_PLUGINS; do
	echo "      $plugin"
done

echo

