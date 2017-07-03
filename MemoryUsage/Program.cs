using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MemoryUsage.EfOverrides;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace MemoryUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Execution with default ParameterExtractingExpressionVisitor");
            Execute(false);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Execution with modified ParameterExtractingExpressionVisitor");
            Execute(true);

            Console.WriteLine();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
        private static void Execute(bool replaceService) {

            var services = new ServiceCollection();
            services.AddMemoryCache();
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<MyTestContext>((serviceProvider, options) =>
                    {
                        options
                            .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                            .UseSqlServer("Data Source=(local)\\SQL2014; Integrated Security=True; Initial Catalog=LSDevTest");

                        if (replaceService)
                        {
                            options.ReplaceService<IQueryCompiler, MyQueryCompiler>();
                        }
                    });

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<MyTestContext>().Database.Migrate();
            }

            Console.WriteLine("Query cache is working, using the dbContext.MyTable2");
            for (int i = 0; i < 5; i++)
            {
                using (var scope = provider.CreateScope())
                {
                    using (var dbContext = scope.ServiceProvider.GetRequiredService<MyTestContext>())
                    {
                        var id = Guid.NewGuid().ToString("N");
                        Console.WriteLine($"Before iteration {i} query cache count {dbContext.CacheCount()}");
                        var items = dbContext.MyTable1.Where(item => dbContext.MyTable2.Where(x => x.Id == item.Id).Where(x => x.Name == id).Any()).ToList();
                        Console.WriteLine($"After iteration {i} query cache count {dbContext.CacheCount()}");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Query cache is not working, using the dbContext.Set<MyTable2>().Where(LambdaExpression)");
            for (int i = 0; i < 5; i++)
            {
                using (var scope = provider.CreateScope())
                {
                    using (var dbContext = scope.ServiceProvider.GetRequiredService<MyTestContext>())
                    {
                        var id = Guid.NewGuid().ToString("N");
                        Expression<Func<MyTable2, bool>> whereExpression = item => item.Name == id;

                        Console.WriteLine($"Before iteration {i} query cache count {dbContext.CacheCount()}");
                        var items = dbContext.Set<MyTable1>().Where(item => dbContext.Set<MyTable2>().Where(x => x.Id == item.Id).Where(whereExpression).Any()).ToList();
                        Console.WriteLine($"After iteration {i} query cache count {dbContext.CacheCount()}");
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Query cache is not working, using the dbContext.Set<MyTable2>().Where(manual created expression)");
            for (int i = 0; i < 5; i++)
            {
                using (var scope = provider.CreateScope())
                {
                    using (var dbContext = scope.ServiceProvider.GetRequiredService<MyTestContext>())
                    {
                        var id = Guid.NewGuid().ToString("N");
                        Expression<Func<string>> valueExpression = () => id;

                        var param = Expression.Parameter(typeof(MyTable2), "item");
                        var property = Expression.Property(param, nameof(MyTable2.Name));
                        Expression result = Expression.Equal(property, Expression.Invoke(valueExpression));
                        var whereExpression = Expression.Lambda<Func<MyTable2, bool>>(result, param);

                        Console.WriteLine($"Before iteration {i} query cache count {dbContext.CacheCount()}");
                        var items = dbContext.Set<MyTable1>().Where(item => dbContext.Set<MyTable2>().Where(x => x.Id == item.Id).Where(whereExpression).Any()).ToList();
                        Console.WriteLine($"After iteration {i} query cache count {dbContext.CacheCount()}");
                    }
                }
            }
        }
    }

    public class MyTestContext : DbContext
    {
        public MyTestContext(DbContextOptions options)
            : base(options)
        {
        }

        public int CacheCount()
        {
            var compiledQueryCache = ((Microsoft.EntityFrameworkCore.Query.Internal.CompiledQueryCache)this.GetService<Microsoft.EntityFrameworkCore.Query.Internal.ICompiledQueryCache>());
            return ((MemoryCache)typeof(Microsoft.EntityFrameworkCore.Query.Internal.CompiledQueryCache).GetTypeInfo().GetField("_memoryCache", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(compiledQueryCache)).Count;
        }
        public DbSet<MyTable1> MyTable1 { get; set; }
        public DbSet<MyTable2> MyTable2 { get; set; }
    }

    public class MyTable1
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class MyTable2
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}