using System;
using System.Text;
using System.Collections.Generic;

using Abakos.Compiler;

namespace Abakos
{
    public static class Abakos
    {
        public static void Main(string [] args)
        {
            StringBuilder raw_expression = new StringBuilder();
            
            foreach(string arg in args) {
                raw_expression.Append(arg);
                raw_expression.Append(" ");
            }
        
            Expression expression = new Expression(args.Length > 0 ? raw_expression.ToString() : "0");
            
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
            
            Console.WriteLine("Executing...\n");
            
            try {
                Stack<Symbol> stack = expression.Evaluate();
                
                Console.WriteLine("");
                Console.WriteLine("Result Stack:");
                
                while(stack.Count > 0) {
                    Console.WriteLine("  {0}", stack.Pop());
                }
            } catch(Exception e) {
                Console.WriteLine("Error handling expression: {0}: {1}", e.GetType().Name, e.Message);
            }
        }
    }
}
