using DataTables.AspNetCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DataTables.AspNetCore
{
    public static class LinqExtensions
    {
        private static PropertyInfo GetPropertyInfo(Type objType, string name)
        {
            var properties = objType.GetProperties();
            var matchedProperty = properties.FirstOrDefault(p => p.Name == name);
            if (matchedProperty == null)
                throw new ArgumentException("name");

            return matchedProperty;
        }
        private static LambdaExpression GetOrderExpression(Type objType, PropertyInfo pi)
        {
            var paramExpr = Expression.Parameter(objType);
            var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
            var expr = Expression.Lambda(propAccess, paramExpr);
            return expr;
        }
        private static LambdaExpression GetWhereExpression(Type objType, PropertyInfo pi, string value)
        {
            var paramExpr = Expression.Parameter(objType);
            var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
            //var expr = Expression.Lambda(propAccess, Expression.Equal(propAccess, Expression.Constant(value));
            return Expression.Lambda(
                    Expression.Equal(
                        Expression.PropertyOrField(paramExpr, pi.Name),
                        Expression.Constant(value.ToString())), paramExpr);
            //return expr;
        }
        public static LambdaExpression GetContainsExpression(Type objType, PropertyInfo pi, string value)
        {
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            var parameterExp = Expression.Parameter(objType);
            var propertyExp = Expression.PropertyOrField(parameterExp, pi.Name);

            var someValue = Expression.Constant(value, typeof(string));
            var containsMethodExp = Expression.Call(propertyExp, method, someValue);

            return Expression.Lambda(containsMethodExp, parameterExp);
        }
        public static Expression ConcatStringExpressions(params Expression[] expressions)
        {
            var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            if (expressions.Count() == 1)
                return expressions[0];
            else
            {
                Expression result = expressions[0];
                for (int i = 1; i < expressions.Count(); i++)
                {
                    result = Expression.Add(result, expressions.ElementAt(i), concatMethod);
                }
                return result;
            }
        }
        public static Expression GetContainsExpression2(ParameterExpression parameterExp, Type objType, PropertyInfo pi, string value)
        {
            MethodInfo method = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });

            var stringConvertMethodInfo =
                typeof(object).GetMethod("ToString");

            //var parameterExp = Expression.Parameter(objType);
            var propertyExp = Expression.PropertyOrField(parameterExp, pi.Name);
            if (pi.PropertyType.ToString().Contains("DateTime"))
            {
                //stringConvertMethodInfo =
                //typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(double?), typeof(int?) });
                //var datetimeDayMethodInfo =
                //    typeof(SqlFunctions).GetMethod("DatePart", new Type[] { typeof(string), pi.PropertyType });

                //var stringReplaceFunctionInfo = typeof(String).GetMethod("Replace", new Type[] { typeof(string), typeof(string) });
                //var LenghtExp2 = Expression.Constant(2, typeof(int?));
                //var LenghtExp4 = Expression.Constant(4, typeof(int?));

                ////DateTime.Now.Year
                ////var testExp = Expression.PropertyOrField(parameterExp, "Year");

                //var ltrimMethodInfo = typeof(SqlFunctions).GetMethod("DatePart", new Type[] { typeof(string), pi.PropertyType });
                //var expDay = Expression.Call(stringConvertMethodInfo, Expression.Convert(Expression.Call(datetimeDayMethodInfo, Expression.Constant("DAY", typeof(string)), Expression.Convert(propertyExp, typeof(DateTime?))), typeof(double?)), LenghtExp2);
                //var expYear = Expression.Call(stringConvertMethodInfo, Expression.Convert(Expression.Call(datetimeDayMethodInfo, Expression.Constant("YEAR", typeof(string)), Expression.Convert(propertyExp, typeof(DateTime?))), typeof(double?)), LenghtExp4);
                //var expMonth = Expression.Call(stringConvertMethodInfo, Expression.Convert(Expression.Call(datetimeDayMethodInfo, Expression.Constant("MONTH", typeof(string)), Expression.Convert(propertyExp, typeof(DateTime?))), typeof(double?)), LenghtExp2);
                //var expHour = Expression.Call(stringConvertMethodInfo, Expression.Convert(Expression.Call(datetimeDayMethodInfo, Expression.Constant("HOUR", typeof(string)), Expression.Convert(propertyExp, typeof(DateTime?))), typeof(double?)), LenghtExp2);
                //var expMinute = Expression.Call(stringConvertMethodInfo, Expression.Convert(Expression.Call(datetimeDayMethodInfo, Expression.Constant("MINUTE", typeof(string)), Expression.Convert(propertyExp, typeof(DateTime?))), typeof(double?)), LenghtExp2);
                //var expSecond = Expression.Call(stringConvertMethodInfo, Expression.Convert(Expression.Call(datetimeDayMethodInfo, Expression.Constant("SECOND", typeof(string)), Expression.Convert(propertyExp, typeof(DateTime?))), typeof(double?)), LenghtExp2);

                //var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                //var dotExpression = Expression.Constant(".", typeof(string));
                //var spaceExpression = Expression.Constant(" ", typeof(string));
                //var dot2Expression = Expression.Constant(":", typeof(string));
                //var zeroExpression = Expression.Constant("0", typeof(string));
                //var expDayMonthYear = ConcatStringExpressions(Expression.Call(ConcatStringExpressions(expDay, dotExpression, expMonth, dotExpression, expYear), stringReplaceFunctionInfo, spaceExpression, zeroExpression), spaceExpression, Expression.Call(ConcatStringExpressions(expHour, dot2Expression, expMinute, dot2Expression, expSecond), stringReplaceFunctionInfo, spaceExpression, zeroExpression));



                return Expression.Call(GetStringConvertExpression(propertyExp, pi.Name), method, Expression.Constant(value));
            }
            else
                if (typeof(string) != pi.PropertyType)
            {
                var expToString = Expression.Call(propertyExp, stringConvertMethodInfo);

                return Expression.Call(expToString, method, Expression.Constant(value));
            }
            else
            {
                var someValue = Expression.Constant(value, typeof(string));
                return Expression.Call(propertyExp, method, someValue);
            }

        }

        public static Expression GetStringConvertExpression(MemberExpression propertyExp, string parameterName)
        {


            var toString = propertyExp.Type.GetMethod("ToString", Type.EmptyTypes);
            return Expression.Call(propertyExp, toString);

            //var stringConvertMethodInfo =
            //    typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(double?) });

            //return Expression.Call(stringConvertMethodInfo, Expression.Convert(propertyExp, typeof(double?)));

        }
        public static object FirstOrDefault<T>(this IList list)
        {
            foreach (var item in list)
                return item;

            return null;
        }
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }
        public static IQueryable<T> WhereContains<T>(this IQueryable<T> query, string name, string value)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            //var expr = GetWhereExpression(typeof(T), propInfo,value);
            var expr = GetContainsExpression(typeof(T), propInfo, value);


            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "Where");
            var genericMethod = method.MakeGenericMethod(typeof(T));
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }
        public static IQueryable<T> WhereContains<T>(this IQueryable<T> query, string[] names, string value)
        {
            Expression expr = null;
            var parameterExp = Expression.Parameter(typeof(T));
            List<Type> infolist = new List<Type>();
            foreach (var name in names)
            {
                var propInfo = GetPropertyInfo(typeof(T), name);
                var resultExp = GetContainsExpression2(parameterExp, typeof(T), propInfo, value);
                if (expr == null)
                {
                    expr = resultExp;
                }
                else
                {
                    expr = Expression.OrElse(expr, resultExp);
                }
            }
            //expr = Expression.Lambda(expr);
            var result = Expression.Lambda<Func<T, bool>>(expr, parameterExp);
            return query.Where(result);

            //var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "Where");
            //var genericMethod = method.MakeGenericMethod(infolist.ToArray());
            //expr = Expression.Call(genericMethod, query.Expression, expr);
            //expr = Expression.Lambda(expr);

            //return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }
        public static IQueryable<TValue> Select<T, TValue>(this IQueryable<T> query, string name, TValue exampleValue)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "Select" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<TValue>)genericMethod.Invoke(null, new object[] { query, expr });
        }
        public static IEnumerable<T> OrderByDescending<T>(this IEnumerable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IEnumerable<T>)genericMethod.Invoke(null, new object[] { query, expr.Compile() });
        }
        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string name)
        {
            var propInfo = GetPropertyInfo(typeof(T), name);
            var expr = GetOrderExpression(typeof(T), propInfo);

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
        }
        public static IQueryable<T> WhereFilter<T>(this IQueryable<T> query, string Name, object value, Type ObjectType, FilterType filterType)
        {
            var param = Expression.Parameter(typeof(T));
            //Equal = 0, NotEqual, Greater, GreaterOrEqueal, Less, LessOrEqual
            Expression expression = null;
            if (!ObjectType.ToString().Contains("DateTime") && !ObjectType.ToString().ToLowerInvariant().Contains("string") && false)
            {
                switch (filterType)
                {
                    case FilterType.Equal:
                        expression = Expression.Equal(GetStringConvertExpression(Expression.PropertyOrField(param, Name), Name),
                      Expression.Constant(value.ToString())); break;
                    case FilterType.NotEqual:
                        expression = Expression.NotEqual(GetStringConvertExpression(Expression.PropertyOrField(param, Name), Name),
           Expression.Constant(value.ToString())); break;
                    case FilterType.Greater:
                        expression = Expression.GreaterThan(GetStringConvertExpression(Expression.PropertyOrField(param, Name), Name),
                Expression.Constant(value.ToString())); break;
                    case FilterType.GreaterOrEqual:
                        expression = Expression.GreaterThanOrEqual(GetStringConvertExpression(Expression.PropertyOrField(param, Name), Name),
     Expression.Constant(value.ToString())); break;
                    case FilterType.Less:
                        expression = Expression.LessThan(GetStringConvertExpression(Expression.PropertyOrField(param, Name), Name),
           Expression.Constant(value.ToString())); break;
                    case FilterType.LessOrEqual:
                        expression = Expression.LessThanOrEqual(GetStringConvertExpression(Expression.PropertyOrField(param, Name), Name),
Expression.Constant(value.ToString())); break;
                }
            }
            else
            {
                switch (filterType)
                {
                    case FilterType.Equal:
                        expression = Expression.Equal(Expression.PropertyOrField(param, Name),
                      Expression.Constant(value)); break;
                    case FilterType.NotEqual:
                        expression = Expression.NotEqual(Expression.PropertyOrField(param, Name),
           Expression.Constant(value.ToString())); break;
                    case FilterType.Greater:
                        expression = Expression.GreaterThan(Expression.PropertyOrField(param, Name),
                Expression.Constant(value)); break;
                    case FilterType.GreaterOrEqual:
                        expression = Expression.GreaterThanOrEqual(Expression.PropertyOrField(param, Name),
     Expression.Constant(value)); break;
                    case FilterType.Less:
                        expression = Expression.LessThan(Expression.PropertyOrField(param, Name),
           Expression.Constant(value)); break;
                    case FilterType.LessOrEqual:
                        expression = Expression.LessThanOrEqual(Expression.PropertyOrField(param, Name),
Expression.Constant(value)); break;
                }
            }

            return query.Where(Expression.Lambda<Func<T, bool>>(expression, param));
        }

        // TODO: You'll need to find out what fields are actually ones you would want to compare on.
        //       This might involve stripping out properties marked with [NotMapped] attributes, for
        //       for example.
        private static IEnumerable<string> GetEntityFieldsToCompareTo<TEntity, TProperty>()
        {
            Type entityType = typeof(TEntity);
            Type propertyType = typeof(TProperty);

            var fields = entityType.GetFields()
                                .Where(f => f.FieldType == propertyType)
                                .Select(f => f.Name);

            var properties = entityType.GetProperties()
                                    .Where(p => p.PropertyType == propertyType)
                                    .Select(p => p.Name);

            return fields.Concat(properties);
        }
    }
}
