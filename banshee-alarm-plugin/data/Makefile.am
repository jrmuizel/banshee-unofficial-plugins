EXTRA_DIST =	\
	$(schema_DATA)

schemadir = $(GCONF_SCHEMA_FILE_DIR)
schema_DATA = alarm.schemas

if GCONF_SCHEMAS_INSTALL
install-data-local:
	if [ -z "$(DESTDIR)" ]; then \
		GCONF_CONFIG_SOURCE="" $(GCONFTOOL) --makefile-install-rule $(srcdir)/$(schema_DATA); \
	fi
endif

MAINTAINERCLEANFILES = Makefile.in
