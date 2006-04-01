MCS_FLAGS = -debug
ASSEMBLY = Scope.dll
SOURCES = \
	$(srcdir)/ScopePlugin.cs \
	$(srcdir)/ScopeView.cs

INCLUDES = -I$(top_srcdir) -Wall -ggdb3 -D_FORTIFY_SOURCE=2 $(GST_CFLAGS)

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY)
plugin_LTLIBRARIES = libscopeplugin.la

libscopeplugin_la_LDFLAGS = -avoid-version -module
libscopeplugin_la_SOURCES = scope-parser.c
libscopeplugin_la_LIBADD = $(GST_LIBS)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in 

