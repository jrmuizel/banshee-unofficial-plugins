MCS_FLAGS = -debug
ASSEMBLY = Recommendation.dll
SOURCES = \
	$(srcdir)/RecommendationPlugin.cs \
	$(srcdir)/RecommendationPane.cs
MENU_RESOURCE = RecommendationMenu.xml
ICON_RESOURCE = play.png

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library -resource:$(srcdir)/$(ICON_RESOURCE),$(ICON_RESOURCE) -resource:$(srcdir)/$(MENU_RESOURCE),$(MENU_RESOURCE) $(BANSHEE_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) $(MENU_RESOURCE)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in 

