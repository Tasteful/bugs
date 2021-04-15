using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BugTest
{
    public class TestRunner
    {
        private readonly Db _db;

        public TestRunner(Db db)
        {
            _db = db;
        }

        public void Run()
        {
            _db.TableA.Where(x => !(x.Name == "Adam" && x.Type == "Person")).ToList();
        }
    }

    public class TableA
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class Db : DbContext
    {
        public Db()
        {
        }

        public Db(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source=(local); Integrated Security=True; Initial Catalog=BugTest;MultipleActiveResultSets=True");
        }

        public DbSet<TableA> TableA { get; set; }
    }
}