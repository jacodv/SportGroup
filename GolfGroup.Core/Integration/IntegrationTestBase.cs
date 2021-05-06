using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using GolfGroup.Api.Helpers;
using GolfGroup.Api.Models;
using GolfGroup.Api.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using Xunit.Abstractions;

namespace GolfGroup.Api.Tests.Integration
{
  public abstract class IntegrationTestBase
  {
    protected IHost _host;
    protected HttpClient _client;
    private TokenModel _loggedInToken;
    protected HttpClient _anonClient;
    protected string BaseUrl { get; }

    protected IntegrationTestBase(string baseUrl, AuthenticateModel authModel=null, ITestOutputHelper output=null)
    {
      if (output != null)
      {
        Log.Logger = new LoggerConfiguration()
          // add the xunit test output sink to the serilog logger
          // https://github.com/trbenning/serilog-sinks-xunit#serilog-sinks-xunit
          .WriteTo.TestOutput(output)
          .CreateLogger();

      }

      BaseUrl = baseUrl;
      var hostBuilder = new HostBuilder()
        .ConfigureWebHost(webHost =>
        {
          // Add TestServer
          webHost.UseTestServer();
          webHost.UseStartup<Startup>();
          webHost.UseConfiguration(new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build());
          webHost.CaptureStartupErrors(true)
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
        });
      _host = hostBuilder.StartAsync().Result;

      // Create a new scope
      using (var scope = _host.Services.CreateScope())
      {
        //Do the migration asynchronously
        DemoDataHelper.Populate(scope.ServiceProvider).Wait();
        DemoDataHelper.SetupOpenIddictCollections(scope.ServiceProvider).Wait();
        DemoDataHelper.SetupIdentities(scope.ServiceProvider).Wait();
      }
      _anonClient = _host.GetTestClient();
      _client = _host.GetTestClient();

      if (authModel == null)
      {
        authModel = new AuthenticateModel()
        {
          UserName = DemoDataHelper.DefaultAdmin, 
          Password = DemoDataHelper.DefaultAdminPassword,
          ClientId = "testclient"
        };
      }
      _loggedInToken = _post<TokenModel>("connect/token", authModel.GetFormData(), useFormEncoding:true).Result;
      _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _loggedInToken.access_token);
    }

    protected static async Task<T> _evaluateResponse<T>(HttpResponseMessage response)
    {
      var content = await response.Content.ReadAsStringAsync();
      if (response.IsSuccessStatusCode)
      {
        return JsonConvert.DeserializeObject<T>(content);
      }
      throw new InvalidOperationException($"Failed: {response.StatusCode}:\n{content}");
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

    protected async Task<T> _post<T>(string url, object payload = null, bool isAnonymous = false, bool useFormEncoding=false)
    {
      var client = isAnonymous ? _anonClient : _client;

      if (payload == null)
        return await _evaluateResponse<T>(await client.PostAsync(url, null));
      
      if(useFormEncoding)
        return await _evaluateResponse<T>(await client.PostAsync(url, new FormUrlEncodedContent((Dictionary<string,string>)payload)));

      return await _evaluateResponse<T>(await client.PostAsync(url, new JsonContent(payload)));
    }
    protected async Task<T> _put<T>(string url, object payload = null)
    {
      if (payload == null)
        return await _evaluateResponse<T>(await _client.PutAsync(url, null));
      return await _evaluateResponse<T>(await _client.PutAsync(url, new JsonContent(payload)));
    }

  }
}