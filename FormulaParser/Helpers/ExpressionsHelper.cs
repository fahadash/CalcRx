using System;
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

        private static Queue<string> leftParameterNames = new Queue<string>();
        private static Queue<string> rightParameterNames = new Queue<string>();

        static ExpressionsHelper()
        {
            var aNames = Enumerable.Range(0, 100)
                .Select(n => "a" + n.ToString());

            foreach (var item in aNames)
            {
                leftParameterNames.Enqueue(item);
            }

            
            var bNames = Enumerable.Range(0, 100)
                .Select(n => "a" + n.ToString());

            foreach (var item in bNames)
            {
                rightParameterNames.Enqueue(item);
            }
        }

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
                Expression calleeA = null;
                Expression calleeB = null;
                ParameterExpression paramA = null;
                ParameterExpression paramB = null;

                Expression valueB = paramB;
                Expression valueA = paramA;

                var sameCreedResult = AreBinaryObservablesOfSameCreed(a, b);

                if (sameCreedResult.IsSameCreed)
                {
                    var callee = sameCreedResult.Creed;
                    var selectorA = sameCreedResult.LeftSelector;
                    var selectorB = sameCreedResult.RightSelector;
                    
                    var resultTypeSameCreed = TypeHelper.GetHigherPrecisionType(genA, genB);
                    valueA = selectorA;
                    valueB = selectorB;

                    if (sameCreedResult.LeftParameter != null && sameCreedResult.RightParameter != null)
                    {
                        selectorB = ReplaceParamInExpressionTree(selectorB, sameCreedResult.RightParameter, sameCreedResult.LeftParameter);
                        valueB = selectorB;
                        paramA = sameCreedResult.LeftParameter;
                    }
                    else if (sameCreedResult.LeftParameter == null)
                    {
                        paramA = sameCreedResult.RightParameter;
                        valueA = paramA;
                        selectorA = paramA; // ReplaceParamInExpressionTree(selectorA, sameCreedResult.LtParameter, sameCreedResult.LeftParameter);
                    }
                    else if (sameCreedResult.RightParameter == null)
                    {// ReplaceParamInExpressionTree(selectorB, sameCreedResult.RightParameter, sameCreedResult.LeftParameter);
                        paramA = sameCreedResult.LeftParameter;
                        valueB = paramA;
                        selectorB = paramA; 
                    }
                    else
                    {
                        paramA  = Expression.Parameter(resultTypeSameCreed);
                        valueA = paramA;
                        valueB = paramA;
                        selectorA = paramA;
                        selectorB = paramB;
                    }

                    resultTypeSameCreed = TypeHelper.GetHigherPrecisionType(valueA.Type, valueB.Type);

                    if (valueA.Type != resultTypeSameCreed)
                    {
                        valueA = Expression.Convert(valueA, resultTypeSameCreed);
                    }

                    if (valueB.Type != resultTypeSameCreed)
                    {
                        valueB = Expression.Convert(valueB, resultTypeSameCreed);
                    }

                    var methodInfo = typeof(Observable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                     .First(m => m.Name == "Select" && m.GetParameters().Count() == 2)
                     .MakeGenericMethod(paramA.Type, resultTypeSameCreed);


                    return Expression.Call(methodInfo,
                            callee, Expression.Lambda(operation(valueA, valueB), paramA));
                }

                var resultType = TypeHelper.GetHigherPrecisionType(genA, genB);

                if (paramB.Type != resultType)
                {
                    valueB = Expression.Convert(paramB, resultType);
                }

                if (paramA.Type != resultType)
                {
                    valueA = Expression.Convert(paramA, resultType);
                }

                var methodInfoSameCreed = typeof(Observable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "CombineLatest" && m.GetParameters().Count() == 3)
            .MakeGenericMethod(genA, genB, resultType);


                return Expression.Call(methodInfoSameCreed,
                        a, b, Expression.Lambda(operation(valueA, valueB), paramA, paramB));
            }
            if (a.IsNumericObservableType() && b.IsNumericType())
            {
                var genA = a.Type.GetFirstObservableGenericType();
                var genB = b.Type;

                ParameterExpression paramA = null;
                Expression callee = null;
                Expression valueA = null;

                if (a is MethodCallExpression)
                {
                    var components = GetSelectCallComponents(a as MethodCallExpression);

                    if (components.CalledOn != null && components.Selector != null)
                    {
                        callee = components.CalledOn;
                        valueA = components.Selector;
                        genA = callee.Type.GetFirstObservableGenericType();
                        paramA = components.Parameter;
                    }
                }

                
                if (callee == null)
                {
                    paramA = Expression.Parameter(genA, leftParameterNames.Dequeue());
                    callee = a;
                    valueA = paramA;
                }

                var resultType = TypeHelper.GetHigherPrecisionType(genA, genB);
                
                if (b.Type != resultType)
                {
                    b = Expression.Convert(b, resultType);
                }
                
                if (valueA.Type != resultType)
                {
                    valueA = Expression.Convert(valueA, resultType);
                }

                var methodInfo = typeof(Observable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                 .First(m => m.Name == "Select" && m.GetParameters().Count() == 2)
                 .MakeGenericMethod(genA, resultType);


                return Expression.Call(methodInfo,
                        callee, Expression.Lambda(operation(valueA, b), paramA));
            }
            if (a.IsNumericType() && b.IsNumericObservableType())
            {
                var genA = a.Type;
                var genB = b.Type.GetFirstObservableGenericType();

                ParameterExpression paramB = null;
                Expression callee = null;
                Expression valueB = null;

                if (b is MethodCallExpression)
                {
                    var components = GetSelectCallComponents(b as MethodCallExpression);

                    if (components.CalledOn != null && components.Selector != null)
                    {
                        callee = components.CalledOn;
                        valueB = components.Selector;
                        genB = callee.Type.GetFirstObservableGenericType();
                        paramB = components.Parameter;
                    }
                }


                if (callee == null)
                {
                    paramB = Expression.Parameter(genB, rightParameterNames.Dequeue());
                    callee = b;
                    valueB = paramB;
                }

                var resultType = TypeHelper.GetHigherPrecisionType(genA, genB);

                if (a.Type != resultType)
                {
                    a = Expression.Convert(a, resultType);
                }


                if (valueB.Type != resultType)
                {
                    valueB = Expression.Convert(valueB, resultType);
                }

                var methodInfo = typeof(Observable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                 .First(m => m.Name == "Select" && m.GetParameters().Count() == 2)
                 .MakeGenericMethod(genB, resultType);


                return Expression.Call(methodInfo,
                        callee, Expression.Lambda(operation(a, valueB), paramB));
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


        class FunctionCallComponents
        {
            internal string MethodName { get; set; }
            internal string CalleeFullName { get; set; }

        }

        class BinaryObservablesOfSameCreed
        {
            internal bool IsSameCreed { get; set; }
            internal Expression Creed { get; set; }

            internal Expression LeftSelector { get; set; }

            internal Expression RightSelector { get; set; }

            internal ParameterExpression LeftParameter { get; set; }

            internal ParameterExpression RightParameter { get; set; }
        }

        class SelectCallComponents : FunctionCallComponents
        {
            internal Expression CalledOn { get; set; }

            internal Expression Selector { get; set; }

            internal ParameterExpression Parameter { get;set; }
        }

        private static FunctionCallComponents GetMethodCallComponents(MethodCallExpression call)
        {
            return new FunctionCallComponents()
            {
                CalleeFullName = call.Method.ReflectedType.FullName,
                MethodName = call.Method.Name,
            };
        }

        private static SelectCallComponents GetSelectCallComponents(MethodCallExpression call)
        {
            var components = new SelectCallComponents()
            {
                CalleeFullName = call.Method.ReflectedType.FullName,
                MethodName = call.Method.Name
            };


            if (components.CalleeFullName == "System.Reactive.Linq.Observable" 
                &&  components.MethodName=="Select")
            {
                var lambda = (call.Arguments[1] as LambdaExpression);

                components.CalledOn = call.Arguments[0];
                components.Selector =lambda.Body;
                components.Parameter = lambda.Parameters[0];
            }

            return components;
        }

        private static BinaryObservablesOfSameCreed AreBinaryObservablesOfSameCreed(Expression left, Expression right)
        {
            Expression leftCreed = null;
            Expression rightCreed = null;
            Expression leftSelector = null;
            Expression rightSelector = null;
            Expression creed = null;

            var result = new BinaryObservablesOfSameCreed();

            if (left is MethodCallExpression)
            {
                var leftCall = GetSelectCallComponents(left as MethodCallExpression);

                leftCreed = leftCall.CalledOn;
                leftSelector = leftCall.Selector;
                result.LeftParameter = leftCall.Parameter;
            }
            else
            {
                result.IsSameCreed = false;

                leftCreed = left;
                leftSelector = left;
            }

            if (right is MethodCallExpression)
            {
                var rightCall = GetSelectCallComponents(right as MethodCallExpression);

                rightCreed = rightCall.CalledOn;
                rightSelector = rightCall.Selector;
                result.RightParameter = rightCall.Parameter;
            }
            else
            {
                result.IsSameCreed = false;
                rightCreed = right;
                rightSelector = right;
            }

            if (leftCreed == rightCreed && leftCreed != null)
            {
                result.IsSameCreed = true;
                result.Creed = leftCreed;
                result.LeftSelector = leftSelector;
                result.RightSelector = rightSelector;
            }

            return result;
        }

        internal static Expression ReplaceParamInExpressionTree(Expression param, ParameterExpression expressionToReplace, ParameterExpression replaceWith)
        {
            if (param is BinaryExpression)
            {
                var exp = param as BinaryExpression;

                Func<Expression, Expression, Expression> operation = null;

                if (exp.NodeType == ExpressionType.Add)
                {
                    operation = Expression.Add;
                }
                else if (exp.NodeType == ExpressionType.Subtract)
                {
                    operation = Expression.Subtract;
                }
                else if (exp.NodeType == ExpressionType.Multiply)
                {
                    operation = Expression.Multiply;
                }
                else if (exp.NodeType == ExpressionType.Divide)
                {
                    operation = Expression.Divide;
                }
                else if (exp.NodeType == ExpressionType.Power)
                {
                    operation = Expression.Power;
                }
                else if (exp.NodeType == ExpressionType.Modulo)
                {
                    operation = Expression.Modulo;
                }
                else
                {
                    throw new ArgumentException("I don't know what is " + exp.NodeType);
                }

                return operation(
                    ReplaceParamInExpressionTree(exp.Left, expressionToReplace, replaceWith),
                ReplaceParamInExpressionTree(exp.Right, expressionToReplace, replaceWith));
                
            }
            else if (param is UnaryExpression)
            {
                var exp = param as UnaryExpression;
                if (exp.NodeType == ExpressionType.Convert)
                {
                    var type = exp.Type;
                    return Expression.Convert(
                        ReplaceParamInExpressionTree(exp.Operand, expressionToReplace, replaceWith),
                        type);
                }
            }
            else if (param is ParameterExpression)
            {
                if (param == expressionToReplace)
                {
                    return replaceWith;
                }
            }
            else if (param is ConstantExpression)
            {
                return param;
            }


            return param;
        }
    }
}
