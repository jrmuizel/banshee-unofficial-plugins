AC_INIT(README)

AC_CANONICAL_SYSTEM
AC_PREREQ(2.13)

VERSION=0.11.0
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
MCS="$MONOBIN/mcs"
GMCS="$MONOBIN/gmcs"

AC_MSG_CHECKING([for mono.pc])
if test -z `$PKG_CONFIG --variable=prefix mono`; then
	AC_MSG_ERROR([missing the mono.pc file, usually found in the mono-devel package])
else
	AC_MSG_RESULT([found])
fi

required_assemblies="Mono.Data.SqliteClient Mono.Posix System.Runtime.Remoting System.Web System.Web.Services"

for asm in $required_assemblies; do
	AC_MSG_CHECKING([for $asm.dll])
	if test ! -e `$PKG_CONFIG --variable=prefix mono`/lib/mono/1.0/$asm.dll; then
		AC_MSG_ERROR([missing required Mono DLL: $asm.dll])
	else
		AC_MSG_RESULT([found])
	fi
done

if test ! -x "$MONO"; then
	AC_MSG_ERROR([No Mono runtime found])
fi

CS="C#"
if test ! -x "$MCS"; then
	AC_MSG_ERROR([No $CS compiler found])
fi

if test ! -x "$GMCS"; then
	AC_MSG_ERROR([No $CS 2.0 (gmcs) compiler found])
fi

AC_SUBST(MCS)
AC_SUBST(GMCS)
AC_SUBST(MONO)

MCS_FLAGS="-debug"

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
PKG_CHECK_MODULES(BANSHEE, banshee >= 0.10.10)
AC_SUBST(BANSHEE_LIBS)

if test "x`$PKG_CONFIG --variable=pluginapi banshee`" = "x2"; then
	banshee_0_10="no"
	AM_CONDITIONAL(BANSHEE_0_10, false)
else
	banshee_0_10="yes"
	MCS_FLAGS="$MCS_FLAGS -define:BANSHEE_0_10"
	AM_CONDITIONAL(BANSHEE_0_10, true)
fi

AC_SUBST(MCS_FLAGS)

dnl Banshee Plugins Path

SYSTEM_PLUGINS=`$PKG_CONFIG --variable=systemplugindir banshee`
REL_PLUGINS_DIR=`basename $SYSTEM_PLUGINS`
REL_BANSHEE_DIR=`echo "$SYSTEM_PLUGINS" | sed -e "s|\/$REL_PLUGINS_DIR||"`
REL_BANSHEE_DIR=`basename $REL_BANSHEE_DIR`
REL_PLUGINS_DIR=$REL_BANSHEE_DIR/$REL_PLUGINS_DIR

expanded_libdir=`( case $prefix in NONE) prefix=$ac_default_prefix ;; *) ;; esac
		   case $exec_prefix in NONE) exec_prefix=$prefix ;; *) ;; esac
		   eval echo $libdir )`
		   
pluginsdir=$libdir/$REL_PLUGINS_DIR
expanded_pluginsdir=$expanded_libdir/$REL_PLUGINS_DIR

AC_SUBST(expanded_libdir)
AC_SUBST(pluginsdir)

if test ! -d "$expanded_libdir/$REL_BANSHEE_DIR"; then
	AC_MSG_ERROR([Banshee installation directory not found. Ensure you are using the same prefix into which Banshee is installed.])
fi

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

AC_ARG_ENABLE(smart-playlists, AC_HELP_STRING(--disable-smart-playlists, [Do not build smart playlists plugin]), enable_smart_playlists=no, enable_smart_playlists=yes)

if test "x$enable_smart_playlists" = "xyes"; then 
	if test "x$banshee_0_10" = "xno"; then	
		ENABLED_PLUGINS="$ENABLED_PLUGINS SmartPlaylists"
	else
		AC_MSG_ERROR([The Smart Playlists plugin requires Banshee 0.11.0-cvs or better. Please pass --disable-smart-playlists to configure.])
	fi
	
	AM_CONDITIONAL(ENABLE_SMART_PLAYLISTS, test "x$enable_smart_playlists" = "xyes")
else
	AM_CONDITIONAL(ENABLE_SMART_PLAYLISTS, false)
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
plugins/Makefile
plugins/MiniMode/Makefile
plugins/MiniMode/AssemblyInfo.cs
plugins/Radio/Makefile
plugins/Radio/AssemblyInfo.cs
plugins/Recommendation/Makefile
plugins/Recommendation/AssemblyInfo.cs
plugins/SmartPlaylists/Makefile
plugins/SmartPlaylists/AssemblyInfo.cs
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

