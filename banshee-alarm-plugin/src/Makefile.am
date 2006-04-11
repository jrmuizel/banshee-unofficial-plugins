MCS_FLAGS = -debug
ASSEMBLY = Alarm.dll
SOURCES = \
	$(srcdir)/Plugin.cs \
	$(srcdir)/VolumeFade.cs \
	$(srcdir)/Alarm.cs \
	$(srcdir)/AlarmConfigPage.cs \
	$(srcdir)/SleepTimerConfigDialog.cs

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb
DISTCLEANFILES = Makefile.in
