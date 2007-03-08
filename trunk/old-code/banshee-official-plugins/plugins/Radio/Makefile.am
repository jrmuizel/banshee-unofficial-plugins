CUSTOM_MCS_FLAGS = $(MCS_FLAGS) \
	-unsafe \
	-resource:$(srcdir)/radio.png,radio.png \
	-resource:$(srcdir)/RadioActions.xml,RadioActions.xml

ASSEMBLY = Radio.dll
ASSEMBLY_GCONF_SCHEMA=banshee-plugin-radio.schemas.in
SOURCES = \
	$(srcdir)/RadioPlugin.cs \
	$(srcdir)/RadioSource.cs \
	$(srcdir)/RadioTrackInfo.cs \
	$(srcdir)/StationManager.cs \
	$(srcdir)/StationModel.cs \
	$(srcdir)/StationView.cs \
	$(srcdir)/CellRendererStation.cs \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/totem-plparser/*.cs \
	$(top_srcdir)/plugins/Common.cs

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(CUSTOM_MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(GTKSHARP_LIBS) $(SOURCES)

include $(top_srcdir)/build/gconf-schema-rules

EXTRA_DIST = $(SOURCES) RadioActions.xml radio.png AssemblyInfo.cs.in $(ASSEMBLY_GCONF_SCHEMA)
CLEANFILES = *.dll *.mdb
MAINTAINERCLEANFILES = Makefile.in *.pidb
DISTCLEANFILES = $(schema_DATA)

