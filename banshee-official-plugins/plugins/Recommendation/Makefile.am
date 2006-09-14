ASSEMBLY = Recommendation.dll
SOURCES = \
	$(srcdir)/RecommendationPlugin.cs \
	$(srcdir)/RecommendationPane.cs \
	$(srcdir)/RecommendationUtilities.cs \
	$(srcdir)/AssemblyInfo.cs \
	$(top_srcdir)/plugins/Common.cs

MENU_RESOURCE = RecommendationMenu.xml
NO_RESULTS_RESOURCE = no-results.png

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library -resource:$(srcdir)/$(MENU_RESOURCE),$(MENU_RESOURCE) -resource:$(srcdir)/$(NO_RESULTS_RESOURCE),$(NO_RESULTS_RESOURCE) $(BANSHEE_LIBS) $(SOURCES) -r:System.Web

EXTRA_DIST = $(SOURCES) $(MENU_RESOURCE) $(NO_RESULTS_RESOURCE) $(srcdir)/AssemblyInfo.cs.in
CLEANFILES = *.dll *.mdb
DISTCLEANFILES = *.pidb
MAINTAINERCLEANFILES = Makefile.in
