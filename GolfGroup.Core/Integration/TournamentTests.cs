using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using FluentAssertions;
using GolfGroup.Api.Controllers;
using GolfGroup.Api.Helpers;
using GolfGroup.Api.Models;
using GolfGroup.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Newtonsoft.Json;
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


      // Update action
      insertedTournament.Date = newTournament.Date.AddDays(5);
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
      var deleteResponse = await _client.DeleteAsync($"{BaseUrl}/{insertedTournament.Id}");
      deleteResponse.IsSuccessStatusCode.Should().BeTrue();
      var getResponse = await _client.GetAsync($"{BaseUrl}/{insertedTournament.Id}");
      getResponse.IsSuccessStatusCode.Should().BeTrue();
      getResponse.Content.ReadAsStringAsync().Result.Should().BeNullOrEmpty();
    }

    private async Task<string> _getFirstGroup()
    {
      var groups = await _evaluateResponse<IList<string>>(await _client.GetAsync(ControllerRoutes.Group));
      var groupModel = await _evaluateResponse<GroupModel>(await _client.GetAsync($"{ControllerRoutes.Group}/name/{groups.First()}"));

      return groupModel.Id;
    }
  }
}