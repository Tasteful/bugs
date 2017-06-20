using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Litium.Application.Common
{
    internal static class ExpressionExtensions
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> _containsMethod = new ConcurrentDictionary<Type, MethodInfo>();

        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }

        public static Expression<Func<T, bool>> AndAlsoNot<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, Expression.Not(right)), parameter);
        }

        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left, right), parameter);
        }

        internal static Expression<Func<TValue, bool>> ContainsDb<TKey, TValue>(this IEnumerable<TKey> keys, Expression<Func<TValue, TKey>> memberPropertyExpression)
        {
            var keyCollection = keys.ToList();
            if (keyCollection.Count < 2000)
            {
                Func<int, bool> fillFunc = count =>
                {
                    if (keyCollection.Count >= count)
                    {
                        return false;
                    }
                    while (keyCollection.Count < count)
                    {
                        keyCollection.Add(default(TKey));
                    }
                    return true;
                };

                foreach (var c in new[] { 1, 5, 10, 20, 50, 75, 100, 150, 200, 250, 500, 750, 1000, 1250, 1400, 1550, 1700, 1850, 2000 })
                {
                    if (fillFunc(c))
                    {
                        break;
                    }
                }

                BinaryExpression last = null;
                foreach (var key in keyCollection)
                {
                    var k = key;
                    Expression<Func<TKey>> idLambda = () => k;
                    var keyExp = Expression.Equal(memberPropertyExpression.Body, ReferenceEquals(k, default(TKey)) ? Expression.Constant(null) : idLambda.Body);
                    last = last == null ? keyExp : Expression.OrElse(last, keyExp);
                }
                Debug.Assert(last != null, "last != null");
                return Expression.Lambda<Func<TValue, bool>>(last, memberPropertyExpression.Parameters);
            }

            var contains = Expression.Call(
                Expression.Constant(keyCollection),
                _containsMethod.GetOrAdd(typeof(TKey), t => typeof(ICollection<TKey>).GetMethod("Contains", new[] { typeof(TKey) })),
                memberPropertyExpression.Body);

            return Expression.Lambda<Func<TValue, bool>>(contains, memberPropertyExpression.Parameters);
        }

        private class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _newValue;
            private readonly Expression _oldValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                {
                    return _newValue;
                }
                return base.Visit(node);
            }
        }
    }
}
