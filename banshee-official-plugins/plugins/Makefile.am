SUBDIRS = 

if ENABLE_MINIMODE
SUBDIRS += MiniMode
endif

if ENABLE_RADIO
SUBDIRS += Radio
endif

if ENABLE_RECOMMENDATION
SUBDIRS += Recommendation
endif

if ENABLE_SMART_PLAYLISTS
if !BANSHEE_0_10
SUBDIRS += SmartPlaylists
endif
endif

MAINTAINERCLEANFILES = Makefile.in

