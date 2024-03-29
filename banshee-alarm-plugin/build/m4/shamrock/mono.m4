AC_DEFUN([SHAMROCK_FIND_MONO_1_0_COMPILER],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MCS, mcs)
])

AC_DEFUN([SHAMROCK_FIND_MONO_2_0_COMPILER],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MCS, gmcs)
])

AC_DEFUN([SHAMROCK_FIND_MONO_RUNTIME],
[
	SHAMROCK_FIND_PROGRAM_OR_BAIL(MONO, mono)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_MODULE],
[
	PKG_CHECK_MODULES(MONO_MODULE, mono >= $1)
])

AC_DEFUN([_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES],
[
	for asm in $(echo "$*" | cut -d, -f2- | sed 's/\,/ /g')
	do
		AC_MSG_CHECKING([for Mono $1 GAC for $asm.dll])
		if test \
			-e "$($PKG_CONFIG --variable=libdir mono)/mono/$1/$asm.dll" -o \
			-e "$($PKG_CONFIG --variable=prefix mono)/lib/mono/$1/$asm.dll"; \
			then \
			AC_MSG_RESULT([found])
		else
			AC_MSG_RESULT([not found])
			AC_MSG_ERROR([missing required Mono $1 assembly: $asm.dll])
		fi
	done
])

AC_DEFUN([SHAMROCK_CHECK_MONO_1_0_GAC_ASSEMBLIES],
[
	_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES(1.0, $*)
])

AC_DEFUN([SHAMROCK_CHECK_MONO_2_0_GAC_ASSEMBLIES],
[
	_SHAMROCK_CHECK_MONO_GAC_ASSEMBLIES(2.0, $*)
])
