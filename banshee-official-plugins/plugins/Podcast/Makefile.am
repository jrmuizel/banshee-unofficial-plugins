ASSEMBLY = Podcast.dll
ASSEMBLY_GCONF_SCHEMA = banshee-plugin-podcast.schemas.in
SOURCES = \
	$(top_srcdir)/plugins/Common.cs \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/FeedUtils.cs \
	$(srcdir)/PodcastCertificatePolicy.cs \
	$(srcdir)/PodcastCore.cs \
	$(srcdir)/PodcastDBManager.cs \
	$(srcdir)/PodcastFeedFetcher.cs \
	$(srcdir)/PodcastFeedInfo.cs \
	$(srcdir)/GConfSchemas.cs \
	$(srcdir)/PodcastInfo.cs \
	$(srcdir)/PodcastLibrary.cs \
	$(srcdir)/PodcastPlugin.cs \
	$(srcdir)/StringUtils.cs \
	$(srcdir)/DownloadCore/*.cs \
	$(srcdir)/FeedParsers/*.cs \
	$(srcdir)/UI/*.cs

podcast_resources = \
	data/PodcastMenu.xml \
	data/images/banshee-new.png \
	data/images/document-save-as-16.png \
	data/images/podcast-icon-16.png \
	data/images/podcast-icon-22.png \
	data/images/podcast-icon-48.png

podcast_resources_build = $(foreach res,$(podcast_resources), $(addprefix -resource:,$(srcdir)/$(res)),$(notdir $(res)))

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY) $(ASSEMBLY).mdb

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library -nowarn:0169 -r:System.Data -r:System.Web -r:System.Xml -r:Mono.Posix $(podcast_resources_build) $(BANSHEE_LIBS) $(GTKSHARP_LIBS) $(SOURCES)

include $(top_srcdir)/build/gconf-schema-rules

EXTRA_DIST = $(SOURCES) $(podcast_resources) $(ASSEMBLY_GCONF_SCHEMA)
CLEANFILES = *.dll *.mdb
DISTCLEANFILES = *.pidb $(schema_DATA)
MAINTAINERCLEANFILES = Makefile.in 

