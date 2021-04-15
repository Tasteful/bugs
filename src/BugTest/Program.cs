using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BugTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var startup = new Startup();
            var services = new ServiceCollection();
            startup.ConfigureServices(services);
            ServiceProvider provider = services.BuildServiceProvider();

            try
            {
                using (IServiceScope scope = provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<Db>();
                    db.Database.Migrate();

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
