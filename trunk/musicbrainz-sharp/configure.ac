AC_INIT(README)
AC_CANONICAL_SYSTEM
AM_INIT_AUTOMAKE(musicbrainz-sharp, 0.1)
AM_MAINTAINER_MODE

MONO_REQUIRED_VERSION=1.1.10
PKG_CHECK_MODULES(MONO, mono >= $MONO_REQUIRED_VERSION)

AC_PATH_PROG(MONO, mono, no)
if test "x$MONO" = "xno"; then
	AC_MSG_ERROR([Cannot find "mono" runtime in your PATH])
fi

AC_PATH_PROG(MCS, mcs, no)
if test "x$MCS" = "xno"; then
	AC_MSG_ERROR([Cannot find "mcs" compiler in your PATH])
fi

PKG_CHECK_MODULES(MUSICBRAINZ, libmusicbrainz >= 2.1.1)

AC_ARG_ENABLE(docs, AC_HELP_STRING([--disable-docs], [Do not build documentation]), with_docs=no, with_docs=yes)

if test "x$with_docs" = "xyes"; then
	AC_PATH_PROG(MONODOCER, monodocer, no)
	if test "x$MONODOCER" = "xno"; then
		AC_MSG_ERROR([You need to install monodoc, or pass --disable-docs to configure to skip documentation installation])
	fi

	AC_PATH_PROG(MDASSEMBLER, mdassembler, no)
	if test "x$MDASSEMBLER" = "xno"; then
		AC_MSG_ERROR([You need to install mdassembler, or pass --disable-docs to configure to skip documentation installation])
	fi

	DOCDIR=`$PKG_CONFIG monodoc --variable=sourcesdir`
	AC_SUBST(DOCDIR)
	AM_CONDITIONAL(BUILD_DOCS, true)
else
	AC_MSG_NOTICE([not building Banshee API documentation])
	AM_CONDITIONAL(BUILD_DOCS, false)
fi

AC_ARG_ENABLE(tests, AC_HELP_STRING([--enable-tests], [Whether or not to build various test programs]), , enable_tests="no")
AM_CONDITIONAL(ENABLE_TESTS, test "x$enable_tests" = "xyes")

#PKG_CHECK_MODULES(GTKSHARP, gtk-sharp-2.0 >= 2.3.92)
#AC_SUBST(GTKSHARP_LIBS)

AC_OUTPUT([
	Makefile
	musicbrainz-sharp.pc
	src/Makefile
	src/MusicBrainz.dll.config
	docs/Makefile
])

