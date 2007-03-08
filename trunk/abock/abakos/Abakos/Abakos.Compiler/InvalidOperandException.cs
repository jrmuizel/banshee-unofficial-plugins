using System;

namespace Abakos.Compiler
{
    public class InvalidOperandException : ApplicationException
    {
        public InvalidOperandException() : base()
        {
        }
        
        public InvalidOperandException(Symbol a, Symbol b) : base(String.Format("{0}, {1}", a, b))
        {
        }
        
        public InvalidOperandException(Symbol symbol) : base(symbol.ToString())
        {
        }

        public InvalidOperandException(string op) : base(op)
        {
        }
    }
}
