using System;
using Microsoft.Extensions.DependencyInjection;

namespace BugTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Startup startup = new Startup();

                ServiceCollection services = new ServiceCollection();
                startup.ConfigureServices(services);
                ServiceProvider provider = services.BuildServiceProvider();

                using (IServiceScope scope = provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<TestRunner>().Run();
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
