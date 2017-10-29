using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MemoryUsage
{
    public class MytestContextFactory : IDesignTimeDbContextFactory<MyTestContext>
    {
        public MyTestContext CreateDbContext(string[] args)
        {
            var opt = new DbContextOptionsBuilder<MyTestContext>()
                .UseSqlServer("dummy")
                .Options;

            return new MyTestContext(opt);
        }
    }
}