using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using FluentAssertions;
using GolfGroup.Api.Controllers;
using GolfGroup.Api.Helpers;
using GolfGroup.Api.Models;
using GolfGroup.Api.Tests.Helpers;
using Xunit;

namespace GolfGroup.Api.Tests.Integration
{
  public class PlayerTests: IntegrationTestBase
  {
    private IMapper _mapper;

    public PlayerTests():base(ControllerRoutes.Player)
    {
      var config = new MapperConfiguration(cfg => {
        cfg.AddProfile<PlayerProfile>();
      });

      _mapper = config.CreateMapper();
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
      var newPlayer = CreatePlayerCreateUpdateModel();

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
      await _deleteAndValidate(insertedPlayer.Id, BaseUrl);
    }

    [Fact]
    public async Task GetPlayersForGroup_GivenDemoGroup_ShouldReturnPlayers()
    {
      //arrange
      var demoGroup= await _get<GroupModel>($"{ControllerRoutes.Group}/name/{DemoDataHelper.DemoGroupName}");

      //action
      var players = await _get<IList<PlayerModel>>($"{BaseUrl}/group/{demoGroup.Id}");

      // assert
      players.Any().Should().BeTrue();
    }

    public static PlayerCreateUpdateModel CreatePlayerCreateUpdateModel()
    {
      var newPlayer = Builder<PlayerCreateUpdateModel>
        .CreateNew()
        .With(_ => _.FirstName = Faker.Name.First())
        .With(_ => _.LastName = Faker.Name.Last())
        .With(_ => _.Email = Faker.Internet.Email())
        .With(_ => _.Mobile = Faker.Phone.Number())
        .Build();
      return newPlayer;
    }
  }
}
