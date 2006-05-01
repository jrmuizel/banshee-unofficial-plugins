MCS_FLAGS = -debug
ASSEMBLY = Wikipedia.dll
SOURCES = \
	$(srcdir)/Plugin.cs \
	$(srcdir)/WikipediaPane.cs
MENU_RESOURCE = WikipediaMenu.xml

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).config $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library -pkg:gecko-sharp-2.0 -resource:$(srcdir)/$(MENU_RESOURCE),$(MENU_RESOURCE) $(BANSHEE_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) $(MENU_RESOURCE) Wikipedia.dll.config
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in 

