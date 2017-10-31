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
using System.Collections;
using System.Collections.Generic;

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
                Console.ForegroundColor = ConsoleColor.Red;
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
                        options.ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning))
                            .UseSqlServer("Data Source=(local); Integrated Security=True; Initial Catalog=LSDevTest");
                    });

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MyTestContext>();
                db.Database.Migrate();
                if (!db.MyTable1.Any())
                {
                    for(var i = 0; i <5; i++)
                    {
                        db.MyTable1.Add(new MyTable1 { Name = $"Name1_{i}" });
                        db.MyTable2.Add(new MyTable2 { Name = $"Name2_{i}" });
                    }
                    db.SaveChanges();
                }
            }

            Console.WriteLine("Executing with inner selectFieldExpression as string will work");
            using (var scope = provider.CreateScope())
            {
                using (var dbContext = scope.ServiceProvider.GetRequiredService<MyTestContext>())
                {
                    Expression<Func<MyTable2, string>> selectFieldExpression =
                        x => x.Name;

                    Expression<Func<MyTable1, object>> orderExpression =
                        item => dbContext.MyTable2
                                         .Where(x => x.Id == item.Id)
                                         .Select(selectFieldExpression)
                                         .FirstOrDefault();

                    var items = dbContext.MyTable1
                                         .Include(x => x.Table2)
                                         .OrderBy(orderExpression)
                                         .ToList();
                }
            }

            Console.WriteLine("Executing with inner selectFieldExpression as object will throw exception when any '.Include()' is used.");
            using (var scope = provider.CreateScope())
            {
                using (var dbContext = scope.ServiceProvider.GetRequiredService<MyTestContext>())
                {
                    Expression<Func<MyTable2, object>> selectFieldExpression =
                        x => x.Name;

                    Expression<Func<MyTable1, object>> orderExpression =
                        item => dbContext.MyTable2
                                         .Where(x => x.Id == item.Id)
                                         .Select(selectFieldExpression)
                                         .FirstOrDefault();

                    var items = dbContext.MyTable1
                                         .Include(x => x.Table2)
                                         .OrderBy(orderExpression)
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MyTable1>(b =>
            {
                b.HasMany(x => x.Table2)
                 .WithOne()
                 .HasPrincipalKey(x => x.Id)
                 .HasForeignKey(x => x.Id);
            });
        }
    }

    public class MyTable1
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<MyTable2> Table2 { get; set; }
    }
    public class MyTable2
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}