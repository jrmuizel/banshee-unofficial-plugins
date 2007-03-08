SOURCES = *.cs Abakos.Compiler/*.cs
ASSEMBLY = Abakos.exe
MCS = gmcs -debug
MONO = mono --debug

all: $(ASSEMBLY)

$(ASSEMBLY): $(SOURCES)
	$(MCS) -out:$@ $(SOURCES)

clean:
	rm -f *.pidb $(ASSEMBLY) $(ASSEMBLY).mdb

run: $(ASSEMBLY)
	$(MONO) $(ASSEMBLY)

