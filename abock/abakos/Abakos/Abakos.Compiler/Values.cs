using System;
using System.Collections.Generic;

namespace Abakos.Compiler
{
    public abstract class ValueSymbol : Symbol
    {
        private double value;
    
        protected ValueSymbol(string name) : this(name, 0.0)
        {
        }
        
        protected ValueSymbol(string name, double value) : base(name)
        {
            this.value = value;
        }
        
        public double Value {
            get { return value; }
        }
    }
    
    public class VariableSymbol : ValueSymbol
    {
        public VariableSymbol(string name) : base(name)
        {
        }
        
        public Symbol Resolve(IDictionary<string, double> table)
        {
            if(!table.ContainsKey(Name)) {
                throw new UnknownVariableException(Name);
            }
            
            return new NumberSymbol(table[Name]);
        }
        
        public new static VariableSymbol FromString(string token)
        {
            return new VariableSymbol(token);
        }
    }
    
    public class VoidSymbol : ValueSymbol
    {
        public VoidSymbol() : base("void")
        {
        }
        
        public static new VoidSymbol FromString(string token)
        {
            if(token == "void") {
                return new VoidSymbol();
            }
            
            throw new InvalidTokenException();
        }
    }

    public class NumberSymbol : ValueSymbol
    {
        public NumberSymbol(double value) : base(null, value)
        {
        }

        public override string ToString()
        {
            return Convert.ToString(Value);
        }

        public new static NumberSymbol FromString(string token)
        {
            return new NumberSymbol(Convert.ToDouble(token));
        }
    }
}
