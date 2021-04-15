using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugTest
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddTransient<TestRunner, TestRunner>();

            services.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<Db>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString("D")));
        }
    }
}
