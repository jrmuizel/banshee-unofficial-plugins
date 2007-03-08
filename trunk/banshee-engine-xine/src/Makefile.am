MCS_FLAGS = -debug
ASSEMBLY = XineEngine.dll
SOURCES = \
	$(srcdir)/Xine.cs \
	$(srcdir)/XinePlayerEngine.cs

enginedir = $(DESTDIR)$(BANSHEE_ENGINE_DIR)
engine_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$@ -target:library $(BANSHEE_LIBS) $(GLIB_SHARP_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb
MAINTAINERCLEANFILES = Makefile.in

