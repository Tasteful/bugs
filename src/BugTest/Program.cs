using System;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace BugTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var startup = new Startup(new HostingEnvironment
                {
                    EnvironmentName = "Development",
                    WebRootPath = PlatformServices.Default.Application.ApplicationBasePath,
                    WebRootFileProvider = new PhysicalFileProvider(PlatformServices.Default.Application.ApplicationBasePath)
                });
                using (TestServer.Create(app =>
                {
                    var method = typeof (Startup).GetMethod("Configure");
                    method.Invoke(startup, method.GetParameters().Select(param => param.ParameterType == typeof (IApplicationBuilder) ? app : app.ApplicationServices.GetRequiredService(param.ParameterType)).ToArray());
                }, startup.ConfigureServices))
                {
                    startup.ServiceProvider.GetRequiredService<TestRunner>().Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex);
            }

            Console.Write("Press enter to exit");
            Console.ReadLine();
        }
    }
}
