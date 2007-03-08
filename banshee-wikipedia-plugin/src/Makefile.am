MCS_FLAGS = -debug
ASSEMBLY = Wikipedia.dll
SOURCES = \
	$(srcdir)/WebRequestHandler.cs \
	$(srcdir)/Gui/ContextBrowser.cs \
	$(srcdir)/Gui/ContextPane.cs \
	$(srcdir)/Page/PageDecorator.cs \
	$(srcdir)/Page/PageFooter.cs \
	$(srcdir)/Page/PageHeader.cs \
	$(srcdir)/Page/WikipediaPage.cs \
	$(srcdir)/Page/Body/ErrorPage.cs \
	$(srcdir)/Page/Body/PageBody.cs \
	$(srcdir)/Page/Body/WaitBody.cs \
	$(srcdir)/Page/Footer/CommonPageFooter.cs \
	$(srcdir)/Page/Header/PageGtkHeader.cs \
	$(srcdir)/Page/Header/LyricsHeader.cs \
	$(srcdir)/Page/Header/WikipediaAlbumHeader.cs \
	$(srcdir)/Page/Header/WikipediaArtistHeader.cs \
	$(srcdir)/Page/Header/WikipediaCommonHeader.cs \
	$(srcdir)/Page/Header/WikipediaGenreHeader.cs \
	$(srcdir)/Page/Header/WikipediaSongHeader.cs \
	$(srcdir)/Parser/Parser.cs \
	$(srcdir)/Parser/Lyrics/LyricsParser.cs \
	$(srcdir)/Parser/Wikipedia/RegexWikipediaParser.cs \
	$(srcdir)/Parser/Wikipedia/XPathWikipediaParser.cs \
	$(srcdir)/Parser/Wikipedia/WikipediaParser.cs \
	$(srcdir)/Query/Query.cs \
	$(srcdir)/Query/Lyrics/LyricsQuery.cs \
	$(srcdir)/Query/Wikipedia/WikipediaAlbumQuery.cs \
	$(srcdir)/Query/Wikipedia/WikipediaArtistQuery.cs \
	$(srcdir)/Query/Wikipedia/WikipediaCommonQuery.cs \
	$(srcdir)/Query/Wikipedia/WikipediaGenreQuery.cs \
	$(srcdir)/Query/Wikipedia/WikipediaQuery.cs \
	$(srcdir)/Query/Wikipedia/WikipediaSongQuery.cs \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/Plugin.cs \
	$(srcdir)/QueryController.cs
	
	

RESOURCES = \
	$(srcdir)/../resources/WikipediaMenu.xml \
	$(srcdir)/../resources/minus.png \
	$(srcdir)/../resources/plus.png \
	$(srcdir)/../resources/Wikipedia-logo-small.png \
	$(srcdir)/../resources/lyrics.png
	
EMBEDDED = $(foreach res,$(RESOURCES),$(addprefix -resource:,$(res)),$(notdir $(res))) 
plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).config $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) $(GECKO_FLAGS) -out:$(ASSEMBLY) -target:library -r:System.Web $(EMBEDDED) $(BANSHEE_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) $(MENU_RESOURCE) Wikipedia.dll.config
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in 

