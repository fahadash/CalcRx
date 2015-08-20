using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FormulaParser
{
    internal static class TypeHelper
    {
        internal static bool IsNumericType(this Expression exp)
        {
            return exp.Type.IsNumericType();
        }

        internal static bool IsNumericObservableType(this Expression exp)
        {
            var type = exp.Type;

            if (type.IsInterface && type.Name == "IObservable`1" && type.GenericTypeArguments.First().IsNumericType())
            {
                return true;
            }

            var observableInterfaces = type.GetInterfaces();

            return observableInterfaces
                        .Any(i => i.Name == "IObservable`1" && i.GenericTypeArguments.Length == 1 && i.GenericTypeArguments.First().IsNumericType());
        }
        internal static bool IsObservableType(this Expression exp)
        {
            var type = exp.Type;

            if (type.IsInterface && type.Name == "IObservable`1")
            {
                return true;
            }

            var observableInterfaces = type.GetInterfaces();

            return observableInterfaces
                        .Any(i => i.Name == "IObservable`1" && i.GenericTypeArguments.Length == 1);

        }

        internal static bool IsNumericType(this Type type)
        {
            return type == typeof(int) || type == typeof(decimal) || type == typeof(double);
        }

        internal static Type GetFirstObservableGenericType(this Type type)
        {
            if (type.IsInterface && type.Name == "IObservable`1")
            {
                var genericType = type.GenericTypeArguments.First();

                return genericType;
              
            }

            return type.GetInterfaces()
                            .Where(i => i.Name == "IObservable`1")
                            .Select(o => o.GetGenericArguments().First())
                            .FirstOrDefault();
                            
        }
        internal static Type GetHigherPrecisionType(Type type1, Type type2)
        {
            Type d = typeof(double);
            Type m = typeof(decimal);
            Type i = typeof(int);

            if (type1 == d || type2 == d)
            {
                return d;
            }
            if (type1 == m || type2 == m)
            {
                return m;
            }
            if (type1 == i || type2 == i)
            {
                return i;
            }
            throw new ArgumentException("Unsupported types specified for precision checking");
        }

        internal static bool IsFunction(this Type type)
        {
            return type.IsGenericType && type.Name.StartsWith("Func`");
        }

        internal static int GetNumberOfGenericArguments(this Type type)
        {
            if (type.IsGenericType == false)
            {
                throw new ArgumentException("Type is not generic type");
            }

            return type.GetGenericArguments().Count();
        }
    }
}
