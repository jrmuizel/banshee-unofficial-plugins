MCS_FLAGS = -debug
ASSEMBLY = Wikipedia.dll
SOURCES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/Plugin.cs \
	$(srcdir)/WebRequestHandler.cs \
	$(srcdir)/WikipediaQueryController.cs \
	$(srcdir)/Gui/WikipediaBrowser.cs \
	$(srcdir)/Gui/WikipediaPane.cs \
	$(srcdir)/Parser/RegexWikipediaParser.cs \
	$(srcdir)/Parser/WikipediaParser.cs \
	$(srcdir)/Parser/XPathWikipediaParser.cs \
	$(srcdir)/WikipediaPage/WikipediaDecorator.cs \
	$(srcdir)/WikipediaPage/WikipediaFooter.cs \
	$(srcdir)/WikipediaPage/WikipediaHeader.cs \
	$(srcdir)/WikipediaPage/WikipediaPage.cs \
	$(srcdir)/WikipediaPage/Body/WikipediaBody.cs \
	$(srcdir)/WikipediaPage/Body/WikipediaErrorPage.cs \
	$(srcdir)/WikipediaPage/Body/WikipediaWaitBody.cs \
	$(srcdir)/WikipediaPage/Footer/CommonWikipediaFooter.cs \
	$(srcdir)/WikipediaPage/Header/WikipediaAlbumHeader.cs \
	$(srcdir)/WikipediaPage/Header/WikipediaArtistHeader.cs \
	$(srcdir)/WikipediaPage/Header/WikipediaCommonHeader.cs \
	$(srcdir)/WikipediaPage/Header/WikipediaGenreHeader.cs \
	$(srcdir)/WikipediaPage/Header/WikipediaGtkHeader.cs \
	$(srcdir)/WikipediaPage/Header/WikipediaSongHeader.cs \
	$(srcdir)/WikiQuery/WikipediaAlbumQuery.cs \
	$(srcdir)/WikiQuery/WikipediaArtistQuery.cs \
	$(srcdir)/WikiQuery/WikipediaCommonQuery.cs \
	$(srcdir)/WikiQuery/WikipediaGenreQuery.cs \
	$(srcdir)/WikiQuery/WikipediaQuery.cs \
	$(srcdir)/WikiQuery/WikipediaSongQuery.cs
	
	

RESOURCES = \
	$(srcdir)/../resources/Wikipedia-logo.png \
	$(srcdir)/../resources/WikipediaMenu.xml \
	$(srcdir)/../resources/cubo_verde.png \
	$(srcdir)/../resources/minus.png \
	$(srcdir)/../resources/plus.png 	
EMBEDDED = $(foreach res,$(RESOURCES),$(addprefix -resource:,$(res)),$(notdir $(res))) 
plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).config $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library -pkg:gecko-sharp-2.0 -r:System.Web $(EMBEDDED) $(BANSHEE_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) $(MENU_RESOURCE) Wikipedia.dll.config
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in 

