ASSEMBLY = Alarm.dll
SOURCES = \
	$(srcdir)/Plugin.cs \
	$(srcdir)/VolumeFade.cs \
	$(srcdir)/Alarm.cs \
	$(srcdir)/AlarmConfigDialog.cs \
	$(srcdir)/SleepTimerConfigDialog.cs \
	$(srcdir)/ConfigurationWidget.cs \
	$(srcdir)/AssemblyInfo.cs

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb
DISTCLEANFILES = Makefile.in
