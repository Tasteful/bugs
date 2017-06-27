using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace MemoryUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            var idQueryExpressionInfo = new IdQueryExpressionInfo { Id = "123" };
            var expression1 = CreateExpression(idQueryExpressionInfo);
            var expression2 = CreateExpression(idQueryExpressionInfo);

            var comparer = new ExpressionEqualityComparer();
            var hash1 = comparer.GetHashCode(expression1);
            var hash2 = comparer.GetHashCode(expression2);

            Console.WriteLine("hash1: " + hash1);
            Console.WriteLine("hash2: " + hash2);

            var eq = comparer.Equals(expression1, expression2);
            Console.WriteLine("eq: " + eq);

            expression1 = CreateExpression2(idQueryExpressionInfo);
            expression2 = CreateExpression2(idQueryExpressionInfo);

            hash1 = comparer.GetHashCode(expression1);
            hash2 = comparer.GetHashCode(expression2);

            Console.WriteLine("hash1: " + hash1);
            Console.WriteLine("hash2: " + hash2);

            eq = comparer.Equals(expression1, expression2);
            Console.WriteLine("eq: " + eq);

            expression1 = CreateExpression3(idQueryExpressionInfo);
            expression2 = CreateExpression3(idQueryExpressionInfo);

            hash1 = comparer.GetHashCode(expression1);
            hash2 = comparer.GetHashCode(expression2);

            Console.WriteLine("hash1: " + hash1);
            Console.WriteLine("hash2: " + hash2);

            eq = comparer.Equals(expression1, expression2);
            Console.WriteLine("eq: " + eq);

            Console.WriteLine();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        public class IdQueryExpressionInfo
        {
            public string Id { get; set; }
        }

        public static Expression<Func<MyTable1, bool>> CreateExpression(IdQueryExpressionInfo queryExpressionInfo)
        {
            var param = Expression.Parameter(typeof(MyTable1), "item");
            var property = Expression.Property(param, "Name");

            Expression result = Expression.Equal(property, Expression.Constant(queryExpressionInfo.Id));
            return Expression.Lambda<Func<MyTable1, bool>>(result, param);
        }

        public static Expression<Func<MyTable1, bool>> CreateExpression2(IdQueryExpressionInfo queryExpressionInfo)
        {
            var param = Expression.Parameter(typeof(MyTable1), "item");
            var property = Expression.Property(param, "Name");

            Expression<Func<string>> valueExpression = () => queryExpressionInfo.Id;

            Expression result = Expression.Equal(property, Expression.Invoke(valueExpression));
            return Expression.Lambda<Func<MyTable1, bool>>(result, param);
        }

        public static Expression<Func<MyTable1, bool>> CreateExpression3(IdQueryExpressionInfo queryExpressionInfo)
        {
            return item => item.Name == queryExpressionInfo.Id;
        }
    }

    public class MyTable1
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
