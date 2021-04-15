using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BugTest
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(options => options.AddDebug().AddConsole().SetMinimumLevel(LogLevel.Trace));
            services.AddTransient<TestRunner, TestRunner>();

            services.AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<Db>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString("D")));
        }
    }
}
