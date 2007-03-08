MCS_FLAGS = -debug -nowarn:0169
ASSEMBLY = Locker.dll
SOURCES = \
	$(srcdir)/LockerConfigDialog.cs \
	$(srcdir)/LockerPlugin.cs \
	$(srcdir)/LockerSource.cs \
	$(srcdir)/LockerTrackInfo.cs \
	$(srcdir)/MP3tunes.cs

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY)

RESOURCES = \
	-resource:$(srcdir)/locker-logo.png,locker-logo.png

REFERENCES = \
	-r:System.Web \
	-r:System.Data.dll \
	-r:System.Drawing.dll

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(REFERENCES) $(RESOURCES) $(BANSHEE_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) $(JSON_SOURCES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
MAINTAINERCLEANFILES = Makefile.in 

