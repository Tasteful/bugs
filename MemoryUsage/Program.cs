using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MemoryUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Execute();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e.ToString());
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
        private static void Execute() {

            var services = new ServiceCollection();
            services.AddMemoryCache();
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<MyTestContext>((serviceProvider, options) =>
                    {
                        options
                            .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                            .UseSqlServer("Data Source=(local); Integrated Security=True; Initial Catalog=LSDevTest");
                    });

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<MyTestContext>().Database.Migrate();
            }

            using (var scope = provider.CreateScope())
            {
                using (var dbContext = scope.ServiceProvider.GetRequiredService<MyTestContext>())
                {
                    var items = dbContext.MyTable1
                        .OrderBy(item => dbContext.MyTable2.Where(x => x.Id == item.Id).Select(x => x.Name))
                        .ToList();
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