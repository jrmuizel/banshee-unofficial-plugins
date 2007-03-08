using System;

namespace Abakos.Compiler
{
    public class UnknownVariableException : InvalidTokenException
    {
        public UnknownVariableException(string token) : base(token)
        {
        }
    }
}
