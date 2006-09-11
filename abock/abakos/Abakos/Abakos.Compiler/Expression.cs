using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Abakos.Compiler
{
    public class Expression
    {
        private string raw_expression;
        private Queue<Symbol> infix_queue;
        private Queue<Symbol> postfix_queue;
        
        private static Regex tokenizer_regex = null;
        
        public Expression(string expression)
        {
            raw_expression = expression;
            infix_queue = ToInfixQueue(expression);
            postfix_queue = ToPostfixQueue(infix_queue);
        }
        
        public Queue<Symbol> InfixQueue {
            get { return infix_queue; }
        }
        
        public Queue<Symbol> PostfixQueue {
            get { return postfix_queue; }
        }
        
        public string RawExpression {
            get { return raw_expression; }
        }
        
        public static Queue<Symbol> ToInfixQueue(string expression)
        {
            if(tokenizer_regex == null) {
                StringBuilder expr = new StringBuilder();
                expr.Append("([\\,");
                
                foreach(OperatorSymbol op in OperatorSymbol.Operators) {
                    expr.Append("\\");
                    expr.Append(op.Name);
                }
                
                foreach(GroupSymbol gp in GroupSymbol.GroupSymbols) {
                    expr.Append("\\");
                    expr.Append(gp.Name);
                }
                
                expr.Append("]{1})");
                
                tokenizer_regex = new Regex(expr.ToString(), RegexOptions.IgnorePatternWhitespace);
            }
            
            string [] tokens = tokenizer_regex.Split(expression);
            List<Symbol> symbol_list = new List<Symbol>();
            
            for(int i = 0; i < tokens.Length; i++) {
                string token = tokens[i].Trim();
                if(token == String.Empty) {
                    continue;
                }
                
                Symbol symbol = Symbol.FromString(token);
                symbol_list.Add(symbol);
                
                // I'm too tired to fix this at the stack/execution level right now;
                // injecting void at the parsing level for zero-arg functions
                if(symbol_list.Count >= 3 && GroupSymbol.IsRight(symbol) &&
                    GroupSymbol.IsLeft(symbol_list[symbol_list.Count - 2]) &&
                    symbol_list[symbol_list.Count - 3] is FunctionSymbol) {
                    symbol_list.Insert(symbol_list.Count - 1, new VoidSymbol());
                }
            }
            
            Queue<Symbol> queue = new Queue<Symbol>();
            foreach(Symbol symbol in symbol_list) {
                queue.Enqueue(symbol);
            }
            
            return queue;
        }
        
        public static Queue<Symbol> ToPostfixQueue(Queue<Symbol> infix)
        {
            Stack<Symbol> stack = new Stack<Symbol>();
            Queue<Symbol> postfix = new Queue<Symbol>();
            Symbol temp_symbol;
            
            foreach(Symbol symbol in infix) {
                if(symbol is ValueSymbol) {
                    postfix.Enqueue(symbol);
                } else if(GroupSymbol.IsLeft(symbol)) {
                    stack.Push(symbol);
                } else if(GroupSymbol.IsRight(symbol)) {
                    while(stack.Count > 0) {
                        temp_symbol = stack.Pop();
                        if(GroupSymbol.IsLeft(temp_symbol)) {
                            break;
                        }

                        postfix.Enqueue(temp_symbol);
                    }
                } else {
                    if(stack.Count > 0) {
                        temp_symbol = stack.Pop();
                        while(stack.Count != 0 && OperationSymbol.Compare(temp_symbol, symbol)) {
                            postfix.Enqueue(temp_symbol);
                            temp_symbol = stack.Pop();
                        }
                        
                        if(OperationSymbol.Compare(temp_symbol, symbol)) { 
                            postfix.Enqueue(temp_symbol);
                        } else {
                            stack.Push(temp_symbol);
                        }
                    }
                
                    stack.Push(symbol);
                }
            }
            
            while(stack.Count > 0) {
                postfix.Enqueue(stack.Pop());
            }
            
            return postfix;
        }
        
        public Stack<Symbol> Evaluate()
        {
            return EvaluatePostfix(postfix_queue);
        }
        
        public static Stack<Symbol> EvaluatePostfix(Queue<Symbol> postfix)
        {
            Stack<Symbol> stack = new Stack<Symbol>();
            
            foreach(Symbol current_symbol in postfix) {
                if(current_symbol is ValueSymbol || current_symbol is CommaSymbol) {
                    stack.Push(current_symbol);
                } else if(current_symbol is OperatorSymbol) {
                    Symbol a = stack.Pop();
                    Symbol b = stack.Pop();
                    stack.Push((current_symbol as OperatorSymbol).Evaluate(b, a));
                } else if(current_symbol is FunctionSymbol) {
                    Stack<Symbol> argument_stack = new Stack<Symbol>();
                    
                    if(stack.Count > 0) {
                        Symbol argument = stack.Pop();
                        
                        if(!(argument is CommaSymbol) && !(argument is VoidSymbol)) {
                            argument_stack.Push(argument);
                        } else if(argument is CommaSymbol) {
                            while(argument is CommaSymbol) {
                                argument = stack.Pop();
                                argument_stack.Push(argument);
                                
                                argument = stack.Pop();
                                if(!(argument is CommaSymbol)) {
                                    argument_stack.Push(argument);
                                }
                            }
                        }
                    }
                    
                    stack.Push(FunctionTable.Execute(current_symbol as FunctionSymbol, argument_stack));
                }
            }
            
            return stack;
        }
    }
}
