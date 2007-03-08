MCS_FLAGS = -debug
ASSEMBLY = BeaglePlugin.dll
SOURCES = \
	$(srcdir)/BeaglePlugin.cs \
	$(srcdir)/BeagleImportSource.cs \
	$(srcdir)/BeagleUtil.cs

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(BANSHEE_LIBS) $(BEAGLE_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in 

install-data-hook:
	for ASM in $(BEAGLE_ASSEMBLIES); do \
		$(INSTALL) -m 0755 $$ASM $(DESTDIR)$(plugindir); \
	done;

uninstall-hook:
	for ASM in $(BEAGLE_ASSEMBLIES); do \
		rm -f $(DESTDIR)$(plugindir)/`basename $$ASM`; \
	done;

