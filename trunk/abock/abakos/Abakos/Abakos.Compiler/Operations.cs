using System;

namespace Abakos.Compiler
{
    public abstract class OperationSymbol : Symbol
    {
        private int precedence;
        
        protected OperationSymbol(string name, int precedence) : base(name)
        {
            this.precedence = precedence;   
        }
        
        public int Precedence {
            get { return precedence; }
        }
        
        public static bool Compare(Symbol a, Symbol b)
        {
            OperationSymbol op_a = a as OperationSymbol;
            OperationSymbol op_b = b as OperationSymbol;
            
            if(op_a == null || op_b == null) {
                return false;
            }

            return !(op_a is GroupSymbol) && op_a.Precedence >= op_b.Precedence;
        }
    }

    public class GroupSymbol : OperationSymbol
    {
        public enum GroupDirection {
            Left,
            Right
        }

        public enum GroupType {
            Parenthesis,
            Brace,
            Bracket
        }

        private static readonly GroupSymbol [] symbol_table = {
            new GroupSymbol("(", GroupType.Parenthesis, GroupDirection.Left),
            new GroupSymbol("{", GroupType.Brace, GroupDirection.Left),
            new GroupSymbol("[", GroupType.Bracket, GroupDirection.Left),
            new GroupSymbol(")", GroupType.Parenthesis, GroupDirection.Right),
            new GroupSymbol("}", GroupType.Brace, GroupDirection.Right),
            new GroupSymbol("]", GroupType.Bracket, GroupDirection.Right)
        };
        
        private GroupDirection direction;
        private GroupType type;
        
        public GroupSymbol(string name, GroupType type, GroupDirection direction) : base(name, 5)
        {
            this.direction = direction;
            this.type = type;
        }

        public GroupDirection Direction {
            get { return direction; }
        }

        public GroupType Type {
            get { return type; }
        }
        
        public static GroupSymbol [] GroupSymbols {
            get { return symbol_table; }
        }

        public static new GroupSymbol FromString(string token)
        {
            foreach(GroupSymbol symbol in symbol_table) {
                if(symbol.Name == token) {
                    return symbol;
                }
            }

            throw new InvalidTokenException(token);
        }
        
        public static bool IsLeft(Symbol symbol)
        {
            GroupSymbol group = symbol as GroupSymbol;
            return group != null && group.Direction == GroupSymbol.GroupDirection.Left;
        }
        
        public static bool IsRight(Symbol symbol)
        {
            GroupSymbol group = symbol as GroupSymbol;
            return group != null && group.Direction == GroupSymbol.GroupDirection.Right;
        }
    }

    public class CommaSymbol : OperationSymbol
    {
        private static readonly CommaSymbol symbol = new CommaSymbol();

        public CommaSymbol() : base(",", 0)
        {
        }

        public static new CommaSymbol FromString(string token)
        {
            if(token == symbol.Name) {
                return symbol;
            }

            throw new InvalidTokenException(token);
        }
    }
}
