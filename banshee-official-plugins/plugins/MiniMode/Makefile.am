MCS_FLAGS = -debug -resource:minimode.glade,minimode.glade -r:/usr/lib/banshee/banshee.exe
# This hack on depending on banshee's EXE should be changed to something more sane... but what?

ASSEMBLY = MiniMode.dll
SOURCES = \
	$(srcdir)/MiniModePlugin.cs \
	$(srcdir)/MiniModeWindow.cs \
	$(srcdir)/SourceComboBox.cs

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(GTKSHARP_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) minimode.glade
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
MAINTAINERCLEANFILES = Makefile.in
DISTCLEANFILES = minimode.gladep* minimode.glade.bak

