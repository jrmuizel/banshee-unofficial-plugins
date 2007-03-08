SUBDIRS = data src

MAINTAINERCLEANFILES = \
	config.guess \
	config.h.in \
	config.sub \
	configure \
	depcomp \
	install-sh \
	ltmain.sh \
	Makefile.in \
	missing \
	aclocal.m4

release: distcheck
	svn cp svn+ssh://abock@svn.banshee-project.org/trunk/helix-dbus-server svn+ssh://abock@svn.banshee-project.org/tags/helix-dbus-server-$(VERSION) -m "tag for $(VERSION)"
	scp helix-dbus-server-$(VERSION).tar.gz banshee@banshee-project.org:/home/banshee/public_html/files/helix-dbus-server

