ASSEMBLY = Radio.dll
SOURCES = \
	$(srcdir)/RadioPlugin.cs \
	$(srcdir)/RadioSource.cs \
	$(srcdir)/StationModel.cs \
	$(srcdir)/StationDialog.cs \
	$(srcdir)/StationView.cs \
	$(srcdir)/StationStore.cs \
	$(srcdir)/totem-plparser/*.cs \
	$(srcdir)/AssemblyInfo.cs

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES) radioactions.xml radio.glade
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -nowarn:0169 -unsafe -target:library $(GTKSHARP_LIBS) $(BANSHEE_LIBS) -resource:$(srcdir)/radio.glade,radio.glade -resource:$(srcdir)/radioactions.xml,radioactions.xml -resource:$(srcdir)/stations.xml,stations.xml $(SOURCES)

EXTRA_DIST = $(SOURCES) $(srcdir)/radioactions.xml $(srcdir)/stations.xml $(srcdir)/radio.glade $(srcdir)/AssemblyInfo.cs.in
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb *.gladep *.bak
DISTCLEANFILES = Makefile.in 

fix-totempl:
	for i in totem-plparser/*.cs; do cat $i | sed -e 's/totem-plparser/totem-plparser.so.1/g' > $i.t && mv $i.t $i; done;

