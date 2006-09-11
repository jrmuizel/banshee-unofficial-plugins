using System;
using System.Reflection;

namespace Abakos.Compiler
{
    public abstract class Symbol
    {    
        private struct SymbolResolve 
        {
            public Type Type;
            public MethodInfo Method;
            
            public SymbolResolve(Type type)
            {
                Type = type;
                Method = null;
            }
        }
    
        private static SymbolResolve [] symbol_resolution_table = {
            new SymbolResolve(typeof(OperatorSymbol)),
            new SymbolResolve(typeof(NumberSymbol)),
            new SymbolResolve(typeof(GroupSymbol)),
            new SymbolResolve(typeof(CommaSymbol)),
            new SymbolResolve(typeof(VoidSymbol)),
            new SymbolResolve(typeof(FunctionSymbol))
        };
    
        private string name;
        
        protected Symbol(string name)
        {
            this.name = name;
        }

        public string Name {
            get { return name; }
        }

        public override string ToString()
        {
            return name;
        }

        public static Symbol FromString(string token)
        {
            for(int i = 0; i < symbol_resolution_table.Length; i++) {
                SymbolResolve sr = symbol_resolution_table[i];
                
                if(sr.Method == null) {
                    sr.Method = sr.Type.GetMethod("FromString");
                    if(sr.Method == null) {
                        throw new ApplicationException(sr.Type + " does not have a FromString method");
                    }
                    
                    symbol_resolution_table[i].Method = sr.Method;
                }
                
                try {
                    return (Symbol)sr.Method.Invoke(null, new object [] { token });
                } catch {
                }
            }
            
            throw new InvalidTokenException(token);
        }
    }
}
