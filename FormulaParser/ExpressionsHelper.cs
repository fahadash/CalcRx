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
        internal static Expression SignMultiply(Expression exp, int sign)
        {
            if (sign == 1)
            {
                return exp;
            }

            return ArithmeticOperation(exp, Expression.Constant(sign), Expression.Multiply);
        }
        internal static Expression Add(Expression a, Expression b)
        {
            return ArithmeticOperation(a, b, Expression.Add);
        }

        internal static Expression Subtract(Expression a, Expression b)
        {
            return ArithmeticOperation(a, b, Expression.Subtract);
        }


        internal static Expression Multiply(Expression a, Expression b)
        {
            return ArithmeticOperation(a, b, Expression.Multiply);
        }

        internal static Expression Divide(Expression a, Expression b)
        {
            return ArithmeticOperation(a, b, Expression.Divide);
        }

        internal static Expression Mod(Expression a, Expression b)
        {
            return ArithmeticOperation(a, b, Expression.Modulo);
        }

        internal static Expression Exponent(Expression a, Expression b)
        {
            return ArithmeticOperation(a, b, Expression.Power);
        }
        internal static Expression ArithmeticOperation(Expression a, Expression b, Func<Expression, Expression, Expression> operation)
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
                        a, b, Expression.Lambda(operation(paramA, paramB), paramA, paramB));
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
                        a, Expression.Lambda(operation(valueA, b), paramA));
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
                        b, Expression.Lambda(operation(a, valueB), paramB));
            }
            else if (a.IsNumericType() && b.IsNumericType())
            {
                return operation(a, b);
            }

            return null;
        }

    }
}
