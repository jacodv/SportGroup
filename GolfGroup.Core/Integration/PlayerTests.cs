using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using GolfGroup.Api.Controllers;
using GolfGroup.Api.Helpers;
using GolfGroup.Api.Models;
using GolfGroup.Api.Tests.Helpers;
using Newtonsoft.Json;
using Xunit;

namespace GolfGroup.Api.Tests.Integration
{
  public class PlayerTests: IntegrationTestBase
  {
    public PlayerTests():base(ControllerRoutes.Player)
    {
      
    }
    [Fact]
    public async Task GetPlayers_GivenNoArguments_ShouldReturnPlayers()
    {
      //Action
      var playersResponse = await _client.GetStringAsync(BaseUrl);

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
      var insertedPlayer = await _evaluateResponse<PlayerModel>(await _client.PostAsync(BaseUrl, new JsonContent(newPlayer)));
      insertedPlayer.Id.Should().NotBeEmpty();
      insertedPlayer.FirstName.Should().Be(newPlayer.FirstName);

      // Update action
      insertedPlayer.FirstName = "Updated";
      var updatedPLayer = await _evaluateResponse<PlayerModel>(await _client.PutAsync($"{BaseUrl}/{insertedPlayer.Id}", new JsonContent(insertedPlayer)));
      updatedPLayer.FirstName.Should().Be(insertedPlayer.FirstName);
      var getPlayer = await _evaluateResponse<PlayerModel>(await _client.GetAsync($"{BaseUrl}/{insertedPlayer.Id}"));
      getPlayer.FirstName.Should().Be(insertedPlayer.FirstName);

      // Delete action
      var deleteResponse = await _client.DeleteAsync($"{BaseUrl}/{insertedPlayer.Id}");
      deleteResponse.IsSuccessStatusCode.Should().BeTrue();
      var getResponse = await _client.GetAsync($"{BaseUrl}/{insertedPlayer.Id}");
      getResponse.IsSuccessStatusCode.Should().BeTrue();
      getResponse.Content.ReadAsStringAsync().Result.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetPlayersForGroup_GivenDemoGroup_ShouldReturnPlayers()
    {
      //arrange
      var demoGroup= await _get<GroupModel>($"{ControllerRoutes.Group}/name/{DemoDataHelper.DemoGroupName}");

      //action
      var players = await _get<IList<Player>>($"{BaseUrl}/group/{demoGroup.Id}");

      // assert
      players.Any().Should().BeTrue();
    }
  }
}
