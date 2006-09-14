SUBDIRS = 

if ENABLE_MINIMODE
SUBDIRS += MiniMode
endif

# disabled for 0.11.0 release, there's a major 
# crasher bug here 
#if ENABLE_RADIO
#SUBDIRS += Radio
#endif

if ENABLE_RECOMMENDATION
SUBDIRS += Recommendation
endif

if ENABLE_PODCAST
SUBDIRS += Podcast
endif

MAINTAINERCLEANFILES = Makefile.in
CLEANFILES = *.dll *.mdb
EXTRA_DIST = Common.cs.in
 
