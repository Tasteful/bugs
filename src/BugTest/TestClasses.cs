using System.Diagnostics.CodeAnalysis;
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
            
        }
    }

    public class Db : DbContext
    {
        public Db([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }
    }
}