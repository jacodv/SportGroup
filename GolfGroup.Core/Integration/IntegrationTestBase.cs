using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using GolfGroup.Api.Helpers;
using GolfGroup.Api.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace GolfGroup.Api.Tests.Integration
{
  public abstract class IntegrationTestBase
  {
    protected IHost _host;
    protected HttpClient _client;
    protected string BaseUrl { get; }

    protected IntegrationTestBase(string baseUrl)
    {
      BaseUrl = baseUrl;
      var hostBuilder = new HostBuilder()
        .ConfigureWebHost(webHost =>
        {
          // Add TestServer
          webHost.UseTestServer();
          webHost.UseStartup<Startup>();
        });

      _host = hostBuilder.StartAsync().Result;

      // Create a new scope
      using (var scope = _host.Services.CreateScope())
      {
        //Do the migration asynchronously
        DemoDataHelper.Populate(scope.ServiceProvider).Wait();
      }

      _client = _host.GetTestClient();
    }

    protected static async Task<T> _evaluateResponse<T>(HttpResponseMessage response)
    {
      if (response.IsSuccessStatusCode)
      {
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
      }
      throw new InvalidOperationException($"Failed: {response.StatusCode}:\n{await response.Content.ReadAsStringAsync()}");
    }

    protected async Task<T> _get<T>(string url)
    {
      return await _evaluateResponse<T>(await _client.GetAsync(url));
    }

    protected async Task _deleteAndValidate(string id, string baseUrl)
    {
      var deleteResponse = await _client.DeleteAsync($"{baseUrl}/{id}");
      deleteResponse.IsSuccessStatusCode.Should().BeTrue();
      var getResponse = await _client.GetAsync($"{baseUrl}/{id}");
      getResponse.IsSuccessStatusCode.Should().BeTrue();
      getResponse.Content.ReadAsStringAsync().Result.Should().BeNullOrEmpty();
    }

    protected async Task<T> _post<T>(string url, object payload=null)
    {
      if (payload == null)
        return await _evaluateResponse<T>(await _client.PostAsync(url, null));
      return await _evaluateResponse<T>(await _client.PostAsync(url, new JsonContent(payload)));
    }
  }
}