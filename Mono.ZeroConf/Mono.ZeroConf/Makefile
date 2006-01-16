LIB_SOURCES = *.cs
LIB = Mono.ZeroConf.dll
TEST_SOURCES = ../Test/ZeroConfTest.cs
TEST = Mono.ZeroConf_test.exe
MONO = mono --debug 
MCS = mcs -debug

all: $(LIB) $(TEST)

$(LIB): $(LIB_SOURCES)
	$(MCS) -out:$@ -target:library $(LIB_SOURCES)

$(TEST): $(TEST_SOURCES)
	$(MCS) -out:$@ -r:$(LIB) $(TEST_SOURCES)

clean: 
	rm -f $(TEST) $(TEST).mdb $(LIB) $(LIB).mdb *.pidb

run:
	$(MONO) $(TEST)



