ASSEMBLY = Recommendation.dll
SOURCES = \
	$(srcdir)/RecommendationPlugin.cs \
	$(srcdir)/RecommendationPane.cs \
	$(srcdir)/AssemblyInfo.cs \
	$(top_srcdir)/plugins/Common.cs

MENU_RESOURCE = RecommendationMenu.xml

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library -resource:$(srcdir)/$(MENU_RESOURCE),$(MENU_RESOURCE) $(BANSHEE_LIBS) $(SOURCES) -r:System.Web

EXTRA_DIST = $(SOURCES) $(MENU_RESOURCE) $(ICON_RESOURCE) $(srcdir)/AssemblyInfo.cs.in
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in *.pidb

