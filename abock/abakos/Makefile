SUBDIRS = Abakos
EXTRA_DIST = Abakos.mds

all:
	for dir in ${SUBDIRS} ; do ( cd $$dir ; ${MAKE} all ) ; done

run:
	for dir in ${SUBDIRS} ; do ( cd $$dir ; ${MAKE} run ) ; done

clean:
	for dir in ${SUBDIRS} ; do ( cd $$dir ; ${MAKE} clean ) ; done

dist: clean
	mkdir -p _dist/Abakos
	rm -rf _dist/Abakos/*
	cp -rf $(SUBDIRS) $(EXTRA_DIST) Makefile _dist/Abakos
	(cd _dist && tar cfz Abakos.tar.gz Abakos && mv Abakos.tar.gz ..)
	rm -rf _dist

