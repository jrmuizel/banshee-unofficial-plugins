using System;

namespace Abakos.Compiler
{
    public class InvalidTokenException : ApplicationException
    {
        public InvalidTokenException() : base()
        {
        }

        public InvalidTokenException(string token) : base(token)
        {
        }
    }
}
