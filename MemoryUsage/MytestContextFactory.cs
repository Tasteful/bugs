using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MemoryUsage
{
    public class MytestContextFactory : IDbContextFactory<MyTestContext>
    {
        public MyTestContext Create(DbContextFactoryOptions options)
        {
            var opt = new DbContextOptionsBuilder<MyTestContext>()
                .UseSqlServer("dummy")
                .Options;

            return new MyTestContext(opt);
        }
    }
}