using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace MemoryUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            services.AddEntityFrameworkSqlServer()
                    .AddDbContext<MyTestContext>((serviceProvider, options) => options
                        .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                        .UseSqlServer("Data Source=(local)\\SQL2014; Integrated Security=True; Initial Catalog=LSDevTest"));

            var provider = services.BuildServiceProvider();
            using (var scope = provider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<MyTestContext>().Database.Migrate();
            }

            using (var scope = provider.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<MyTestContext>())
                {
                    Console.WriteLine($"Cache count before first CreateExpression {context.CacheCount()}");
                    context.MyTable1.Where(CreateExpression(new IdQueryExpressionInfo { Id = "123-1" })).ToList();
                    Console.WriteLine($"Cache count before second CreateExpression {context.CacheCount()}");
                    context.MyTable1.Where(CreateExpression(new IdQueryExpressionInfo { Id = "123-2" })).ToList();
                    Console.WriteLine($"Cache count before third CreateExpression {context.CacheCount()}");
                    context.MyTable1.Where(CreateExpression(new IdQueryExpressionInfo { Id = "123-2-1" })).ToList();
                    Console.WriteLine($"Cache count after third CreateExpression {context.CacheCount()}");

                    Console.WriteLine();
                    Console.WriteLine();

                    Console.WriteLine($"Cache count before first CreateExpression2 {context.CacheCount()}");
                    context.MyTable1.Where(CreateExpression2(new IdQueryExpressionInfo { Id = "123-3" })).ToList();
                    Console.WriteLine($"Cache count before second CreateExpression2 {context.CacheCount()}");
                    context.MyTable1.Where(CreateExpression2(new IdQueryExpressionInfo { Id = "123-4" })).ToList();
                    Console.WriteLine($"Cache count before third CreateExpression2 {context.CacheCount()}");
                    context.MyTable1.Where(CreateExpression2(new IdQueryExpressionInfo { Id = "123-4-1" })).ToList();
                    Console.WriteLine($"Cache count after third CreateExpression2 {context.CacheCount()}");

                    Console.WriteLine();
                    Console.WriteLine();

                    Console.WriteLine($"Cache count before first CreateExpression3 {context.CacheCount()}");
                    context.MyTable1.Where(CreateExpression3(new IdQueryExpressionInfo { Id = "123-5" })).ToList();
                    Console.WriteLine($"Cache count before second CreateExpression3 {context.CacheCount()}");
                    context.MyTable1.Where(CreateExpression3(new IdQueryExpressionInfo { Id = "123-6" })).ToList();
                    Console.WriteLine($"Cache count before third CreateExpression3 {context.CacheCount()}");
                    context.MyTable1.Where(CreateExpression3(new IdQueryExpressionInfo { Id = "123-6-1" })).ToList();
                    Console.WriteLine($"Cache count after third CreateExpression3 {context.CacheCount()}");

                    Console.WriteLine();
                }
            }
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

    public class MyTestContext : DbContext
    {
        public MyTestContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MyTable1> MyTable1 { get; set; }

        public int CacheCount()
        {
            var compiledQueryCache = (CompiledQueryCache)this.GetService<ICompiledQueryCache>();
            return ((MemoryCache)typeof(CompiledQueryCache).GetTypeInfo().GetField("_memoryCache", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(compiledQueryCache)).Count;
        }
    }
    public class MyTable1
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
