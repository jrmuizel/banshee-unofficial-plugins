CUSTOM_MCS_FLAGS = $(MCS_FLAGS) -resource:smart-playlist.glade,smart-playlist.glade -resource:source-smart-playlist.png,source-smart-playlist.png
ASSEMBLY = SmartPlaylists.dll
SOURCES = \
	$(srcdir)/Plugin.cs \
	$(srcdir)/Editor.cs \
	$(srcdir)/SmartPlaylist.cs \
	$(srcdir)/QueryBuilder.cs \
	$(srcdir)/QueryBuilderModel.cs \
	$(srcdir)/AssemblyInfo.cs

SYSTEM_REFERENCES = \
	System.Data.dll

SYSTEM_REFERENCES_BUILD = $(addprefix -r:, $(SYSTEM_REFERENCES))

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb $(ASSEMBLY).config

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(CUSTOM_MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(GTKSHARP_LIBS) $(SYSTEM_REFERENCES_BUILD) $(SOURCES)

EXTRA_DIST = $(SOURCES) $(ASSEMBLY).config smart-playlist.glade source-smart-playlist.png $(srcdir)/AssemblyInfo.cs.in
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in 

