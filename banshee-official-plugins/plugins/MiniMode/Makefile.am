CUSTOM_MCS_FLAGS = $(MCS_FLAGS) -resource:$(srcdir)/minimode.glade,minimode.glade
ASSEMBLY = MiniMode.dll
SOURCES = \
	$(srcdir)/MiniModePlugin.cs \
	$(srcdir)/MiniModeWindow.cs \
	$(srcdir)/SourceComboBox.cs \
	$(srcdir)/SourceModel.cs \
	$(srcdir)/AssemblyInfo.cs \
	$(top_srcdir)/plugins/Common.cs

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(CUSTOM_MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(GTKSHARP_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) minimode.glade $(srcdir)/AssemblyInfo.cs.in
CLEANFILES = *.dll *.mdb
MAINTAINERCLEANFILES = Makefile.in *.pidb
DISTCLEANFILES = minimode.gladep* minimode.glade.bak

