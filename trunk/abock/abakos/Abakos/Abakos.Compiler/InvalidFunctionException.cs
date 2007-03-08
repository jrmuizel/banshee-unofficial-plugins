using System;

namespace Abakos.Compiler
{
    public class InvalidFunctionException : InvalidTokenException
    {
        public InvalidFunctionException(string token) : base(token)
        {
        }
    }
}
