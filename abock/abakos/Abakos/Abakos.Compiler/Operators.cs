using System;

namespace Abakos.Compiler
{
    public abstract class OperatorSymbol : OperationSymbol
    {
        private readonly static OperatorSymbol [] symbol_table = {
            new PowOperatorSymbol(),
            new DivOperatorSymbol(),
            new MulOperatorSymbol(),
            new ModOperatorSymbol(),
            new AddOperatorSymbol(),
            new SubOperatorSymbol()
        };
        
        public OperatorSymbol(string name, int precedence) : base(name, precedence)
        {
        }
        
        public virtual Symbol Evaluate(Symbol a, Symbol b)
        {
            try {
                return Evaluate((ValueSymbol)a, (ValueSymbol)b);
            } catch {
                throw new InvalidOperandException(a, b);
            }
        }
        
        protected virtual Symbol Evaluate(ValueSymbol a, ValueSymbol b)
        {
            return new NumberSymbol(0);
        }

        public static OperatorSymbol [] Operators { 
            get { return symbol_table; }
        }

        public static new OperatorSymbol FromString(string token)
        {
            foreach(OperatorSymbol symbol in symbol_table) {
                if(symbol.Name == token) {
                    return symbol;
                }
            }

            throw new InvalidTokenException(token);
        }
    }
    
    public class PowOperatorSymbol : OperatorSymbol
    {
        public PowOperatorSymbol() : base("^", 3)
        {
        }
        
        protected override Symbol Evaluate(ValueSymbol a, ValueSymbol b)
        {
            return new NumberSymbol(Math.Pow(a.Value, b.Value));
        }
    }
    
    public class DivOperatorSymbol : OperatorSymbol
    {
        public DivOperatorSymbol() : base("/", 2)
        {
        }
        
        protected override Symbol Evaluate(ValueSymbol a, ValueSymbol b)
        {
            return new NumberSymbol(a.Value / b.Value);
        }
    }
    
    public class MulOperatorSymbol : OperatorSymbol
    {
        public MulOperatorSymbol() : base("*", 2)
        {
        }
        
        protected override Symbol Evaluate(ValueSymbol a, ValueSymbol b)
        {
            return new NumberSymbol(a.Value * b.Value);
        }
    }
    
    public class ModOperatorSymbol : OperatorSymbol
    {
        public ModOperatorSymbol() : base("%", 2)
        {
        }
        
        protected override Symbol Evaluate(ValueSymbol a, ValueSymbol b)
        {
            return new NumberSymbol((int)a.Value % (int)b.Value);
        }
    }
    
    public class AddOperatorSymbol : OperatorSymbol
    {
        public AddOperatorSymbol() : base("+", 1)
        {
        }
        
        protected override Symbol Evaluate(ValueSymbol a, ValueSymbol b)
        {
            return new NumberSymbol(a.Value + b.Value);
        }
    }
    
    public class SubOperatorSymbol : OperatorSymbol
    {
        public SubOperatorSymbol() : base("-", 1)
        {
        }
        
        protected override Symbol Evaluate(ValueSymbol a, ValueSymbol b)
        {
            return new NumberSymbol(a.Value - b.Value);
        }
    }
}
