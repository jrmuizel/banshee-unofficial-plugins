DOC_UPDATER = @MONODOCER@ -delete
DOC_ASSEMBLER = @MDASSEMBLER@ --out musicbrainz-sharp-docs --ecma

ASSEMBLY = $(top_builddir)/src/MusicBrainz.dll

if BUILD_DOCS
docdir = $(DOCDIR)
doc_DATA = \
	musicbrainz-sharp-docs.zip \
	musicbrainz-sharp-docs.tree \
	musicbrainz-sharp-docs.source
endif

update-docs: $(ASSEMBLY)
	$(DOC_UPDATER) -assembly:$(ASSEMBLY) -path:en

musicbrainz-sharp-docs.zip musicbrainz-sharp-docs.tree: $(srcdir)/en/*/*.xml
	$(DOC_ASSEMBLER) $(srcdir)/en

EXTRA_DIST = \
	/en/*.xml \
	en/*/*.xml \
	musicbrainz-sharp-docs.source 

DISTCLEANFILES = Makefile.in musicbrainz-sharp-docs.tree musicbrainz-sharp-docs.zip 

