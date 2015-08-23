﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FormulaParser.Helpers
{
    internal class ExpressionsHelper
    {
        internal static List<Function> functions;

        internal static Expression FunctionCall(string functionName, IEnumerable<Expression> args)
        {
            var argList = args.ToList();
            var name = string.Format("{0}({1})", functionName, argList.Count);

            var function = functions.Where(f => f.FunctionName.Equals(name)).First();


            var method = function.FunctionExpression.Type.GetMethod("Invoke");
            var exp = Expression.Call(function.FunctionExpression, method, args);

            //var exp = Expression.Lambda(function.FunctionExpression, args.OfType<ParameterExpression>());

            return exp;
        }

        internal static Expression PropertyAccess(Expression exp, string name)
        {
            //TODO: Write code to see if type has that property

            if (exp.IsObservableType())
            {
                var gen = exp.Type.GetFirstObservableGenericType();
                var param = Expression.Parameter(gen);

                var property = gen.GetProperties()
                                    .Where(p => p.Name.Equals(name))
                                    .FirstOrDefault();

                if (property == null)
                {
                    // throw custom exception here
                }

                var resultType = property.PropertyType;

                var methodInfo = typeof(Observable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                 .First(m => m.Name == "Select" && m.GetParameters().Count() == 2)
                 .MakeGenericMethod(gen, resultType);

                return Expression.Call(methodInfo,
                     exp, Expression.Lambda(Expression.Property(param, property), param));
            }

            return Expression.Property(exp, name);
        }
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
            if (a == null)
            {
                return ArithmeticOperation(b, Expression.Constant(-1), Expression.Multiply);
            }

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
                var resultType = TypeHelper.GetHigherPrecisionType(a.Type, b.Type);
                if (a.Type != resultType)
                {
                    a = Expression.Convert(a, resultType);
                }
                if (b.Type != resultType)
                {
                    b = Expression.Convert(b, resultType);
                }

                return operation(a, b);
            }

            return null;
        }

    }
}