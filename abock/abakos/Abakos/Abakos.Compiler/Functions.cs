using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Abakos.Compiler
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public class FunctionAttribute : Attribute
    {
        private string name;
        
        public FunctionAttribute(string name)
        {
            this.name = name;
        }
        
        public string Name {
            get { return name; }
            set { name = value; }
        }
    }

    public static class FunctionTable
    {
        internal struct FunctionCache
        {
            public string Name;
            public bool RawSymbols;
            public MethodInfo Method;
            public int ArgumentCount;
        }
        
        private static List<Assembly> assemblies = new List<Assembly>();
        private static Dictionary<string,FunctionCache> cached_functions;
         
        static FunctionTable()
        {
            assemblies.Add(Assembly.GetExecutingAssembly());
        }
        
        public static IEnumerable<FunctionCache> Functions {
            get { return cached_functions.Values; }
        }
         
        public static void AddAssembly(Assembly assembly)
        {
            if(!assemblies.Contains(assembly)) {
                assemblies.Add(assembly);
            }
        }
        
        private static void LoadFunctionCache()
        {
            cached_functions = new Dictionary<string,FunctionCache>();
        
            foreach(Assembly assembly in assemblies) {
                foreach(Type type in assembly.GetTypes()) {
                    foreach(MethodInfo method in type.GetMethods()) {
                        foreach(Attribute attr in method.GetCustomAttributes(typeof(FunctionAttribute), true)) {
                            FunctionAttribute attribute = attr as FunctionAttribute;
                            if(attribute == null) {
                                continue;
                            }
                            
                            LoadFunction(attribute, method);
                        }
                    }
                }
            }
        }
        
        private static void LoadFunction(FunctionAttribute attribute, MethodInfo method)
        {
            ParameterInfo [] parameters = method.GetParameters();

            FunctionCache cache;
            cache.ArgumentCount = parameters == null ? 0 : parameters.Length;
            cache.Name = attribute.Name;
            cache.Method = method;
            cache.RawSymbols = method.ReturnType == typeof(Symbol);

            if(parameters != null) {
                if(parameters.Length == 1 && parameters[0].ParameterType.IsArray && 
                    parameters[0].ParameterType.HasElementType && 
                    parameters[0].ParameterType.GetElementType() == method.ReturnType) {
                    cache.ArgumentCount = -1;
                } else {
                    foreach(ParameterInfo parameter in parameters) {
                        if(parameter.ParameterType != method.ReturnType) {
                            throw new ArgumentException(String.Format(
                                "'{0}': return and parameter types must be the same",
                                attribute.Name));
                        }
                    }
                }
            }

            cached_functions.Add(cache.Name, cache);
        }
        
        public static Symbol Execute(FunctionSymbol symbol, Stack<Symbol> arguments)
        {
            if(cached_functions == null) {
                LoadFunctionCache();
            }
            
            if(!cached_functions.ContainsKey(symbol.Name)) {
                throw new InvalidFunctionException(symbol.Name);
            }
            
            FunctionCache function = cached_functions[symbol.Name];
            if(arguments.Count != function.ArgumentCount && function.ArgumentCount >= 0) {
                throw new ArgumentException(symbol.Name);
            }
            
            if(function.RawSymbols) {
                if(function.ArgumentCount == -1) {
                    return (Symbol)function.Method.Invoke(null, new object [] { arguments.ToArray() });
                } else {
                    return (Symbol)function.Method.Invoke(null, arguments.ToArray());
                }
            }
            
            ArrayList resolved_arguments = new ArrayList();
            
            while(arguments.Count > 0) {
                Symbol argument = arguments.Pop();
                if(!(argument is ValueSymbol)) {
                    throw new ArgumentException("argument must be ValueSymbol");
                }
                resolved_arguments.Add((argument as ValueSymbol).Value);
            }
            
            double retval;
            
            if(function.ArgumentCount == -1) {
                retval = (double)function.Method.Invoke(null, new object [] { resolved_arguments.ToArray(typeof(double)) });
            } else {
                retval = (double)function.Method.Invoke(null, resolved_arguments.ToArray());
            }
            
            return new NumberSymbol(retval);
        }
    }

    public class FunctionSymbol : OperationSymbol
    {
        public FunctionSymbol(string name) : base(name, 4)
        {
        }
        
        public static new FunctionSymbol FromString(string token)
        {
            return new FunctionSymbol(token);
        }
    }
    
    public static class MathFunctions
    {
        [Function("sin")]
        public static double Sin(double a)
        {
            return Math.Sin(a);
        }
        
        [Function("cos")]
        public static double Cos(double a)
        {
            return Math.Cos(a);
        }
        
        [Function("sum")]
        public static double Sum(double [] args)
        {
            if(args == null) {
                return 0.0;
            }
            
            double sum = 0.0;
            
            foreach(double arg in args) {
                sum += arg;
            }
            
            return sum;
        }
        
        [Function("PI")]
        public static double PI()
        {
            return Math.PI;
        }
        
        [Function("E")]
        public static double E()
        {
            return Math.E;
        }
        
        [Function("functions")]
        public static Symbol RuntimeAvailable()
        {
            foreach(FunctionTable.FunctionCache function in FunctionTable.Functions) {
                Console.WriteLine("{0} ({1})", function.Name, 
                    function.ArgumentCount == -1 ? "variable" : function.ArgumentCount.ToString());
            }
            
            return new VoidSymbol();
        }
    }
}
