using System;
using System.Collections.Generic;

using Abakos.Compiler;

namespace Abakos
{
    public static class Abakos
    {
        public static void Main(string [] args)
        {
            Expression expression = new Expression(args.Length > 0 ? args[0] : "0");
            
            Console.Write("Infix: ");
            foreach(Symbol token in expression.InfixQueue) {
                Console.Write("{0} ", token);
            }
            Console.WriteLine("");

            Console.Write("Postfix: ");
            foreach(Symbol token in expression.PostfixQueue) {
                Console.Write("{0} ", token);
            }
            Console.WriteLine("");
            
            Console.WriteLine("Result Stack:");
            Stack<Symbol> stack = expression.Evaluate();
            while(stack.Count > 0) {
                Console.WriteLine("  {0}", stack.Pop());
            }
        }
    }
}
