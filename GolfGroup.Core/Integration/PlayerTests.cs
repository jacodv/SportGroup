using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using GolfGroup.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Xunit;

namespace GolfGroup.Api.Tests.Integration
{
  public class PlayerTests: IntegrationTestBase
  {
    [Fact]
    public async Task GetPlayers_GivenNoArguments_ShouldReturnPlayers()
    {
      //Action
      var playersResponse = await _client.GetStringAsync("/api/player");

      //Assert
      playersResponse.Should().Contain("[");
    }

    [Fact]
    public async Task FullCycle_GivenValidPlayer_ShouldAddUpdateAndRemovePlayer()
    {
      // arrange
      var newPlayer = Builder<PlayerCreateUpdateModel>
        .CreateNew()
        .With(_ => _.FirstName = Faker.Name.First())
        .With(_ => _.LastName = Faker.Name.Last())
        .With(_ => _.Email = Faker.Internet.Email())
        .With(_ => _.Mobile = Faker.Phone.Number())
        .Build();

      // Create action
      var insertedPlayer = await _evaluateReponse<PlayerModel>(await _client.PostAsync("/api/player",new JsonContent(newPlayer)));
      insertedPlayer.Id.Should().NotBeEmpty();
      insertedPlayer.FirstName.Should().Be(newPlayer.FirstName);

      // Update action
      insertedPlayer.FirstName = "Updated";
      var updatedPLayer = await _evaluateReponse<PlayerModel>(await _client.PutAsync($"/api/player/{insertedPlayer.Id}", new JsonContent(insertedPlayer)));
      updatedPLayer.FirstName.Should().Be(insertedPlayer.FirstName);
      var getPlayer = await _evaluateReponse<PlayerModel>(await _client.GetAsync($"/api/player/{insertedPlayer.Id}"));
      getPlayer.FirstName.Should().Be(insertedPlayer.FirstName);

      // Delete action
      var deleteResponse = await _client.DeleteAsync($"/api/player/{insertedPlayer.Id}");
      deleteResponse.IsSuccessStatusCode.Should().BeTrue();
      var getResponse = await _client.GetAsync($"/api/player/{insertedPlayer.Id}");
      getResponse.IsSuccessStatusCode.Should().BeTrue();
      getResponse.Content.ReadAsStringAsync().Result.Should().BeNullOrEmpty();
    }

    private async Task<T> _evaluateReponse<T>(HttpResponseMessage response)
    {
      if (response.IsSuccessStatusCode)
      {
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
      }
      throw new InvalidOperationException($"Failed: {response.StatusCode}:\n{await response.Content.ReadAsStringAsync()}");
    }
  }

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

  public class JsonContent : StringContent
  {
    public JsonContent(object obj) :
      base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
    { }
  }
}
