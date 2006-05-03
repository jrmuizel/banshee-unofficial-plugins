MCS_FLAGS = -debug -resource:smart-playlist.glade,smart-playlist.glade -resource:source-smart-playlist.png,source-smart-playlist.png
ASSEMBLY = SmartPlaylists.dll
SOURCES = \
	$(srcdir)/Plugin.cs \
	$(srcdir)/Editor.cs \
	$(srcdir)/SmartPlaylist.cs \
	$(srcdir)/QueryBuilder.cs \
	$(srcdir)/QueryBuilderModel.cs

SYSTEM_REFERENCES = \
	System.Data.dll

SYSTEM_REFERENCES_BUILD = $(addprefix -r:, $(SYSTEM_REFERENCES))

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb $(ASSEMBLY).config

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(GTKSHARP_LIBS) $(SYSTEM_REFERENCES_BUILD) $(SOURCES)

EXTRA_DIST = $(SOURCES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in 

