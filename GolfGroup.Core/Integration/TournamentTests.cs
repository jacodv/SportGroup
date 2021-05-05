using System;
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
  public class TournamentTests : IntegrationTestBase
  {
    private IMapper _mapper;

    public TournamentTests():base(ControllerRoutes.Tournament)
    {
      var config = new MapperConfiguration(cfg => {
        cfg.AddProfile<TournamentProfile>();
      });

      _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task GetTournaments_GivenNoArguments_ShouldReturnTournaments()
    {
      //Action
      var TournamentsResponse = await _client.GetStringAsync(BaseUrl);

      //Assert
      TournamentsResponse.Should().Contain("[");
    }

    [Fact]
    public async Task FullCycle_GivenValidTournament_ShouldAddUpdateAndRemoveTournament()
    {
      // arrange
      var groupId = await _getFirstGroup();
      var insertedTournament = await _insertAndValidate(groupId);

      // Update action
      insertedTournament.Date = insertedTournament.Date.AddDays(5);
      insertedTournament.Display = "updated";
      var updatedTournament = await _evaluateResponse<TournamentModel>(
        await _client.PutAsync($"{BaseUrl}/{insertedTournament.Id}", new JsonContent(_mapper.Map<TournamentCreateUpdateModel>(insertedTournament))));
      updatedTournament.Dump("updatedTournament");
      updatedTournament.Date.Should().Be(insertedTournament.Date);
      updatedTournament.Display.Should().Be(insertedTournament.Display);


      var getTournament = await _evaluateResponse<TournamentModel>(await _client.GetAsync($"{BaseUrl}/{insertedTournament.Id}"));
      getTournament.Dump("getTournament");
      getTournament.Display.Should().Be(insertedTournament.Display);
      getTournament.Date.Should().Be(insertedTournament.Date);

      // Delete action
      await _deleteAndValidate(insertedTournament.Id, BaseUrl);
    }

    [Fact]
    public async Task AddPlayer_GivenPlayer_ShouldAddPlayer()
    {
      // arrange
      var players = await _get<IList<PlayerModel>>(ControllerRoutes.Player);
      var player = players.Last();
      var groupId = await _getFirstGroup();
      var insertedTournament = await _insertAndValidate(groupId);

      // action
      //todo: use ControllerRoutes
      var tournament = await _post<TournamentModel>($"{BaseUrl}/{insertedTournament.Id}/player/{player.Id}");

      // assert
      tournament.Players.Any(_ => _.Id == player.Id).Should().BeTrue();
      await _deleteAndValidate(tournament.Id, BaseUrl);
    }

    [Fact]
    public async Task AddGuest_GivenGuest_ShouldAddGuest()
    {
      // arrange
      var player = new GuestCreateUpdateModel()
      {
        Name = Faker.Name.FullName(),
        Email = Faker.Internet.Email(),
        Mobile = Faker.Phone.Number()
      };

      var groupId = await _getFirstGroup();
      var insertedTournament = await _insertAndValidate(groupId);

      // action
      //todo: use ControllerRoutes
      var tournament = await _post<TournamentModel>($"{BaseUrl}/{insertedTournament.Id}/guest", player);

      // assert
      tournament.Guests.Any(_ => _.Name == player.Name).Should().BeTrue();
      await _deleteAndValidate(tournament.Id, BaseUrl);
    }

    [Fact]
    public async Task AddPlayers_GivenPlayer_ShouldAddPlayers()
    {
      // arrange
      var players = await _get<IList<PlayerModel>>(ControllerRoutes.Player);
      var selectedPlayerIds = players.Take(5).Select(_=>_.Id).ToList();
      var groupId = await _getFirstGroup();
      var insertedTournament = await _insertAndValidate(groupId);

      // action
      //todo: use ControllerRoutes
      var tournament = await _post<TournamentModel>(
        $"{BaseUrl}/{insertedTournament.Id}/players", 
        selectedPlayerIds);

      // assert
      tournament.Players
        .All(_ => selectedPlayerIds.Any(x=>x==_.Id)).Should().BeTrue();
      await _deleteAndValidate(tournament.Id, BaseUrl);
    }

    [Fact]
    public async Task CalculateFourBallsGiven24Players_ShouldCreate6RandomFourBalls()
    {
      // arrange
      var players = await _get<IList<PlayerModel>>(ControllerRoutes.Player);
      var selectedPlayerIds = players.Take(24).Select(_ => _.Id).ToList();
      var groupId = await _getFirstGroup();
      var insertedTournament = await _insertAndValidate(groupId);
      //todo: use ControllerRoutes
      var tournament = await _post<TournamentModel>(
        $"{BaseUrl}/{insertedTournament.Id}/players",
        selectedPlayerIds);

      // action
      var fourBalls = await _post<List<FourBallModel>>($"{BaseUrl}/{tournament.Id}/calculate");

      // assert
      fourBalls.Count.Should().Be(6);

      await _deleteAndValidate(tournament.Id, BaseUrl);
    }

    [Fact]
    public async Task CalculateFourBallsGiven22Players_ShouldCreate6RandomFourBallsWithTwoGuests()
    {
      // arrange
      var players = await _get<IList<PlayerModel>>(ControllerRoutes.Player);
      var selectedPlayerIds = players.Take(22).Select(_ => _.Id).ToList();
      var groupId = await _getFirstGroup();
      var insertedTournament = await _insertAndValidate(groupId);
      //todo: use ControllerRoutes
      var tournament = await _post<TournamentModel>(
        $"{BaseUrl}/{insertedTournament.Id}/players",
        selectedPlayerIds);

      // action
      var fourBalls = await _post<List<FourBallModel>>($"{BaseUrl}/{tournament.Id}/calculate");

      // assert
      fourBalls.Count.Should().Be(6);
      fourBalls[5].Guests.Count.Should().Be(2);
      await _deleteAndValidate(tournament.Id, BaseUrl);
    }

    private async Task<string> _getFirstGroup()
    {
      var groups = await _evaluateResponse<IList<string>>(await _client.GetAsync(ControllerRoutes.Group));
      var groupModel = await _evaluateResponse<GroupModel>(await _client.GetAsync($"{ControllerRoutes.Group}/name/{groups.First()}"));

      return groupModel.Id;
    }
    private async Task<TournamentModel> _insertAndValidate(string groupId)
    {
      var newTournament = Builder<TournamentCreateUpdateModel>
        .CreateNew()
        .With(_ => _.Date = DateTime.Now.Date)
        .With(_ => _.GroupId = groupId)
        .Build();

      newTournament.Dump("newTournament");

      // Create action
      var insertedTournament = await _evaluateResponse<TournamentModel>(await _client.PostAsync(BaseUrl, new JsonContent(newTournament)));
      insertedTournament.Dump("insertedTournament");
      insertedTournament.Id.Should().NotBeEmpty();
      insertedTournament.Date.Should().Be(newTournament.Date);
      return insertedTournament;
    }

  }
}