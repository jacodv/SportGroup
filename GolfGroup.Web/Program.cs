using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GolfGroup.Api.Helpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GolfGroup.Api
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
        
      try
      {
        Log.Information("Starting up");
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
      catch (Exception ex)
      {
        Log.Fatal(ex, "Application start-up failed");
      }
      finally
      {
        Log.CloseAndFlush();
      }
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
      WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>()
        .CaptureStartupErrors(true)
        .UseSerilog((hostingContext, loggerConfiguration) =>
        {
          loggerConfiguration
            .ReadFrom.Configuration(hostingContext.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
            .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment);

#if DEBUG
          // Used to filter out potentially bad data due debugging.
          // Very useful when doing Seq dashboards and want to remove logs under debugging session.
          loggerConfiguration.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
#endif
        });
  }
}
