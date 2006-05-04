CUSTOM_MCS_FLAGS = $(MCS_FLAGS) -resource:minimode.glade,minimode.glade
ASSEMBLY = MiniMode.dll
SOURCES = \
	$(srcdir)/MiniModePlugin.cs \
	$(srcdir)/MiniModeWindow.cs \
	$(srcdir)/SourceComboBox.cs \
	$(srcdir)/AssemblyInfo.cs

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(CUSTOM_MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(GTKSHARP_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) minimode.glade $(srcdir)/AssemblyInfo.cs.in
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
MAINTAINERCLEANFILES = Makefile.in
DISTCLEANFILES = minimode.gladep* minimode.glade.bak

