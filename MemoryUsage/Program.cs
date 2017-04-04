using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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
                .UseSqlServer("Data Source=(local)\\SQL2014; Integrated Security=True; Initial Catalog=LSDevTest"));

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<MyTestContext>().Database.Migrate();
            }

            Console.WriteLine("Query cache is working, using the dbContext.MyTable1");
            for (int i = 0; i < 5; i++)
            {
                using (var scope = provider.CreateScope())
                {
                    using (var dbContext = scope.ServiceProvider.GetRequiredService<MyTestContext>())
                    {
                        Console.WriteLine($"Bafore iteration {i} query cache count {dbContext.CacheCount()}");
                        var items = dbContext.MyTable1.Where(item => dbContext.MyTable1.Any(x => x.Id == item.Id)).ToList();
                        Console.WriteLine($"After iteration {i} query cache count {dbContext.CacheCount()}");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Query cache is not working, using the dbContext.Set<MyTable1>()");
            for (int i = 0; i < 5; i++)
            {
                using (var scope = provider.CreateScope())
                {
                    using (var dbContext = scope.ServiceProvider.GetRequiredService<MyTestContext>())
                    {
                        Console.WriteLine($"Bafore iteration {i} query cache count {dbContext.CacheCount()}");
                        var items = dbContext.MyTable1.Where(item => dbContext.Set<MyTable1>().Any(x => x.Id == item.Id)).ToList();
                        Console.WriteLine($"After iteration {i} query cache count {dbContext.CacheCount()}");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }

    public class MyTestContext : DbContext {
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
    }

    public class MyTable1
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}