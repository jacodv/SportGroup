using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace GolfGroup.Api.Tests.Integration
{
  public abstract class IntegrationTestBase
  {
    protected IHost _host;
    protected HttpClient _client;

    protected IntegrationTestBase()
    {
      var hostBuilder = new HostBuilder()
        .ConfigureWebHost(webHost =>
        {
          // Add TestServer
          webHost.UseTestServer();
          webHost.UseStartup<Startup>();
        });

      _host = hostBuilder.StartAsync().Result;
      _client = _host.GetTestClient();
    }
  }
}