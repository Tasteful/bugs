using Microsoft.Data.Entity;

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
    }
}