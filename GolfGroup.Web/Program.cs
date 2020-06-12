using System.Threading.Tasks;
using GolfGroup.Api.Helpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace GolfGroup.Api
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var webHost = CreateWebHostBuilder(args).Build();

      // Create a new scope
      using (var scope = webHost.Services.CreateScope())
      {
        //Do the migration asynchronously
        await DemoDataHelper.Populate(scope.ServiceProvider);
      }

      // Run the WebHost, and start accepting requests
      // There's an async overload, so we may as well use it
      await webHost.RunAsync();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
  }
}
