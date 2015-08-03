using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FormulaParser
{
    internal class ExpressionsHelper
    {
        internal static Expression Add(Expression a, Expression b)
        {
            if (a.IsNumericObservableType() && b.IsNumericObservableType())
            {
                var genA = a.Type.GetFirstObservableGenericType();
                var genB = b.Type.GetFirstObservableGenericType();
                var resultType = TypeHelper.GetHigherPrecisionType(genA, genB);

                var paramA = Expression.Parameter(genA);
                var paramB = Expression.Parameter(genB);

                var methodInfo = typeof(Observable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "CombineLatest" && m.GetParameters().Count() == 3)
            .MakeGenericMethod(genA, genB, resultType);


                return Expression.Call(methodInfo,
                        a, b, Expression.Lambda(Expression.Add(paramA, paramB), paramA, paramB));
            }
            if (a.IsNumericObservableType() && b.IsNumericType())
            {
                var genA = a.Type.GetFirstObservableGenericType();
                var genB = b.Type;
                var resultType = TypeHelper.GetHigherPrecisionType(genA, genB);

                var paramA = Expression.Parameter(genA);

                var methodInfo = typeof(Observable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                 .First(m => m.Name == "Select" && m.GetParameters().Count() == 2)
                 .MakeGenericMethod(genA, resultType);


                if (b.Type != resultType)
                {
                    b = Expression.Convert(b, resultType);
                }

                Expression valueA = paramA;

                if (paramA.Type != resultType)
                {
                    valueA = Expression.Convert(paramA, resultType);
                }

                return Expression.Call(methodInfo,
                        a, Expression.Lambda(Expression.Add(valueA, b), paramA));
            }
            if (a.IsNumericType() && b.IsNumericObservableType())
            {
                var genA = a.Type;
                var genB = b.Type.GetFirstObservableGenericType();
                var resultType = TypeHelper.GetHigherPrecisionType(genA, genB);

                var paramB = Expression.Parameter(genB);

                var methodInfo = typeof(Observable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                 .First(m => m.Name == "Select" && m.GetParameters().Count() == 2)
                 .MakeGenericMethod(genB, resultType);

                if (a.Type != resultType)
                {
                    a = Expression.Convert(a, resultType);
                }

                Expression valueB = paramB;

                if (paramB.Type != resultType)
                {
                    valueB = Expression.Convert(paramB, resultType);
                }

                return Expression.Call(methodInfo,
                        b, Expression.Lambda(Expression.Add(valueB, a), paramB));
            }
            else if (a.IsNumericType() && b.IsNumericType())
            {
                return Expression.Add(a, b);
            }

            return null;
        }

    }
}
