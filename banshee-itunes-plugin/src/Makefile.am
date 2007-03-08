MCS_FLAGS = -debug -nowarn:0169
ASSEMBLY = iTunesMusicStore.dll
SOURCES = \
	$(srcdir)/MusicStorePlugin.cs \
	$(srcdir)/MusicStoreSource.cs \
	$(srcdir)/MusicStoreConfigDialog.cs \
	$(srcdir)/MusicStoreView.cs \
	$(srcdir)/MusicStoreTrackInfo.cs \
       	$(srcdir)/MusicStorePurchaseTransaction.cs

FAIRSTORE_SOURCES = \
        $(srcdir)/fairstore/FairStore.cs \
	$(srcdir)/fairstore/iTMS.cs \
	$(srcdir)/fairstore/Utility.cs \
        $(srcdir)/fairstore/MP4.cs

plugindir = $(PLUGINDIR)
plugin_SCRIPTS = $(ASSEMBLY)

REFERENCES = \
	-r:ICSharpCode.SharpZipLib \
	-r:System.Web

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library $(REFERENCES) $(BANSHEE_LIBS) $(SOURCES) $(FAIRSTORE_SOURCES)

EXTRA_DIST = $(SOURCES) $(FAIRSTORE_SOURCES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
DISTCLEANFILES = Makefile.in 

