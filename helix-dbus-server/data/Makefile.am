# Dbus service file
servicedir = $(DBUS_SERVICES_DIR)
service_in_files = org.gnome.HelixDbusPlayer.service.in
service_DATA = $(service_in_files:.service.in=.service)

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = helix-dbus-server.pc

# Rule to make the service file with bindir expanded
$(service_DATA): $(service_in_files) Makefile
	@sed -e "s|\@bindir\@|$(bindir)|" $< > $@

EXTRA_DIST = $(service_in_files)

MAINTAINERCLEANFILES = Makefile.in
CLEANFILES = org.gnome.HelixDbusPlayer.service

