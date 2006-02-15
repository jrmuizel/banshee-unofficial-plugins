ASM_SOURCES = *.cs ../Test/ZeroConfTest.cs
ASM = Mono.ZeroConf.exe
MONO = mono --debug 
MCS = mcs -debug

all: $(ASM) $(TEST)

$(ASM): $(ASM_SOURCES)
	$(MCS) -out:$@ -pkg:gtk-sharp-2.0 $(ASM_SOURCES)

clean: 
	rm -f $(ASM) $(ASM).mdb *.pidb

run:
	$(MONO) $(ASM)



