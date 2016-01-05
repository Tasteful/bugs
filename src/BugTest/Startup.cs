using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BugTest
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
        }

        public IServiceProvider ServiceProvider { get; set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(minLevel: LogLevel.Debug);
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddTransient<TestRunner, TestRunner>();

            services.AddEntityFramework()
                .AddInMemoryDatabase()
                .AddDbContext<Db>(o => o.UseInMemoryDatabase());

            return ServiceProvider = services.BuildServiceProvider();
        }
    }
}