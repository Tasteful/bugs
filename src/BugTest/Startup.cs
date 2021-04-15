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
            services.AddLogging(options =>
            {
                options
                    .ClearProviders()
                    .AddFilter("*", LogLevel.Trace)
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddDebug()
                    .AddConsole();
            });
            services.AddTransient<TestRunner, TestRunner>();

            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<Db>((sp, opt) =>
                {
                    opt.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
                });
        }
    }
}
