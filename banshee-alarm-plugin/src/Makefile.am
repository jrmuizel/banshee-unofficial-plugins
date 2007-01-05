ASSEMBLY = Alarm.dll
ASSEMBLY_GCONF_SCHEMA = banshee-plugin-alarm.schemas.in

SOURCES = \
	$(srcdir)/AlarmPlugin.cs \
	$(srcdir)/VolumeFade.cs \
	$(srcdir)/Alarm.cs \
	$(srcdir)/AlarmConfigDialog.cs \
	$(srcdir)/GConfSchemas.cs \
	$(srcdir)/SleepTimerConfigDialog.cs \
	$(srcdir)/ConfigurationWidget.cs \
	$(srcdir)/AssemblyInfo.cs

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(SOURCES)

include $(top_srcdir)/build/gconf-schema-rules

EXTRA_DIST = $(SOURCES) $(ASSEMBLY_GCONF_SCHEMA)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb
DISTCLEANFILES = Makefile.in $(schema_DATA)
