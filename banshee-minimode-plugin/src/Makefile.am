MCS_FLAGS = -debug
ASSEMBLY = MiniMode.dll
SOURCES = \
	$(srcdir)/MiniModePlugin.cs \
	$(srcdir)/MiniModeWindow.cs

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(GTKSHARP_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) minimode.glade
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
MAINTAINERCLEANFILES = Makefile.in
DISTCLEANFILES = minimode.gladep* minimode.glade.bak

