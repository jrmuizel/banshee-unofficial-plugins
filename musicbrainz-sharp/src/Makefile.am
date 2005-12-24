MCS_FLAGS = -debug
ASSEMBLY_NAME = MusicBrainz
ASSEMBLY = $(ASSEMBLY_NAME).dll

DOC_UPDATER = @MONODOCER@ -delete
DOC_ASSEMBLER = @MDASSEMBLER@ --out musicbrainz-sharp-docs --ecma

musicbrainzdir = $(pkglibdir)
musicbrainz_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb $(ASSEMBLY).config

ASSEMBLY_SOURCES = \
	$(srcdir)/Client.cs \
	$(srcdir)/ClientVersion.cs \
	$(srcdir)/Rdf.cs \
	$(srcdir)/SimpleDisc.cs \
	$(srcdir)/SimpleArtist.cs \
	$(srcdir)/SimpleAlbum.cs \
	$(srcdir)/SimpleTrack.cs \
	$(srcdir)/Utilities.cs

if ENABLE_TESTS
all: $(ASSEMBLY) test
else
all: $(ASSEMBLY)
endif

if BUILD_DOCS
docdir = $(DOCDIR)
doc_DATA = \
	musicbrainz-sharp-docs.zip \
	musicbrainz-sharp-docs.tree \
	musicbrainz-sharp-docs.source
endif

$(ASSEMBLY): $(ASSEMBLY_SOURCES)
	$(MCS) $(MCS_FLAGS) -target:library  -out:$@ $(ASSEMBLY_SOURCES)

update-docs: $(ASSEMBLY)
	$(DOC_UPDATER) -assembly:$(ASSEMBLY) -path:en

musicbrainz-sharp-docs.zip musicbrainz-sharp-docs.tree: $(srcdir)/en/*/*.xml
	$(DOC_ASSEMBLER) $(srcdir)/en

test: simple-disc-test.exe simple-lookup-test.exe

simple-disc-test.exe: SimpleDiscTest.cs 
	$(MCS) $(MCS_FLAGS) -out:$@ -r:$(ASSEMBLY) $<

simple-lookup-test.exe: SimpleLookupTest.cs
	$(MCS) $(MCS_FLAGS) -out:$@ -r:$(ASSEMBLY) $<

EXTRA_DIST = \
	$(ASSEMBLY_SOURCES) \
	$(ASSEMBLY).config.in \
	/en/*.xml \
	en/*/*.xml \
	musicbrainz-sharp-docs.source \
	SimpleDiscTest.cs \
	SimpleLookupTest.cs 

CLEANFILES = $(ASSEMBLY) *.dll *.exe
DISTCLEANFILES = *.mdb Makefile.in musicbrainz-sharp-docs.tree musicbrainz-sharp-docs.zip 

