ASSEMBLY = Podcast.dll
SOURCES = \
	AssemblyInfo.cs \
	FeedUtils.cs \
	PodcastCore.cs \
	PodcastDBManager.cs \
	PodcastFeedFetcher.cs \
	PodcastFeedInfo.cs \
	PodcastGConfKeys.cs \
	PodcastInfo.cs \
	PodcastLibrary.cs \
	PodcastPlugin.cs \
	StringUtils.cs \
	DownloadCore/*.cs \
	FeedParsers/*.cs \
	UI/*.cs

podcast_resources = \
	data/PodcastMenu.xml \
	data/images/dialog-error.png \
	data/images/document-save-as-16.png \
	data/images/edit-delete.png \
	data/images/go-down.png \
	data/images/go-next-grey.png \
	data/images/go-next.png \
	data/images/podcast-icon-16.png \
	data/images/podcast-icon-22.png \
	data/images/podcast-icon-48.png \
	data/images/view-refresh.png

podcast_resources_build = $(foreach res,$(podcast_resources), $(addprefix -resource:,$(top_builddir)/plugins/Podcast/$(res)),$(notdir $(res)))

plugindir = $(pluginsdir)
plugin_SCRIPTS = $(ASSEMBLY)

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) $(MCS_FLAGS) -out:$(ASSEMBLY) -target:library -nowarn:0169 -r:System.Data -r:System.Xml -r:Mono.Posix $(podcast_resources_build) $(BANSHEE_LIBS) $(GTKSHARP_LIBS) $(SOURCES)

EXTRA_DIST = $(SOURCES) $(podcast_resources)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb 
MAINTAINERCLEANFILES = Makefile.in 

