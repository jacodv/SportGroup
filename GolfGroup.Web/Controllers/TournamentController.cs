using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GolfGroup.Api.Interfaces;
using GolfGroup.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace GolfGroup.Api.Controllers
{
  [Route(ControllerRoutes.Tournament)]
  [ApiController]
  public class TournamentController : ControllerBase
  {
    private readonly IRepository<Tournament> _tournaments;
    private readonly IMapper _mapper;
    private readonly IRepository<Group> _groups;
    private readonly IRepository<Player> _players;

    public TournamentController(IRepository<Tournament> tournaments, IMapper mapper, 
      IRepository<Group> groups, IRepository<Player> players)
    {
      _tournaments = tournaments;
      _mapper = mapper;
      _groups = groups;
      _players = players;
    }
    [HttpGet]
    public IEnumerable<TournamentSummaryModel> Get()
    {
      return _mapper.Map<IEnumerable<TournamentSummaryModel>>(source: _tournaments.AsQueryable().AsEnumerable());
    }

    [HttpGet("{id}")]
    public async Task<TournamentModel> Get(string id)
    {
      return _mapper.Map<TournamentModel>(await _tournaments.FindByIdAsync(id));
    }

    [HttpPost]
    public async Task<TournamentModel> Post([FromBody] TournamentCreateUpdateModel value)
    {
      var insert = _mapper.Map<Tournament>(value);
      insert.Group = await _validateAndGetGroup(value.GroupId);
      await _tournaments.InsertOneAsync(insert);
      return _mapper.Map<TournamentModel>(insert);
    }

    [HttpPut("{id}")]
    public async Task<TournamentModel> Put(string id, [FromBody] TournamentCreateUpdateModel value)
    {
      var updated = _mapper.Map<Tournament>(value);
      updated.Id = ObjectId.Parse(id);
      await _tournaments.ReplaceOneAsync(updated);
      return _mapper.Map<TournamentModel>(updated);
    }

    [Route(ControllerRoutes.TournamentAddPlayer)]
    [HttpPost]
    public async Task<TournamentModel> AddPlayer(string id, string playerId)
    {
      var tournament = await _tournaments.FindByIdAsync(id);
      
      if(tournament==null)
        throw new ArgumentOutOfRangeException(nameof(id), "Invalid tournament");
      
      if (tournament.Players.Any(_=>_.Id==ObjectId.Parse(playerId)))
        return _mapper.Map<TournamentModel>(tournament);

      var player = await _players.FindByIdAsync(playerId);
      if(player==null)
        throw new ArgumentOutOfRangeException(nameof(id), "Invalid player");

      tournament.Players.Add(_mapper.Map<PlayerSummary>(player));
      
      await _tournaments.ReplaceOneAsync(tournament);
      return _mapper.Map<TournamentModel>(tournament);
    }

    [Route(ControllerRoutes.TournamentAddGuest)]
    [HttpPost]
    public async Task<TournamentModel> AddGuest(string id, [FromBody] GuestCreateUpdateModel guest)
    {
      var tournament = await _tournaments.FindByIdAsync(id);

      if(guest==null)
        throw new ArgumentNullException(nameof(guest), "Guest is missing");

      if (tournament == null)
        throw new ArgumentOutOfRangeException(nameof(id), "Invalid tournament");

      if (tournament.Guests.Any(_ => _.Name == guest.Name))
        return _mapper.Map<TournamentModel>(tournament);

      tournament.Guests.Add(_mapper.Map<Guest>(guest));
      
      await _tournaments.ReplaceOneAsync(tournament);
      return _mapper.Map<TournamentModel>(tournament);
    }

    [Route(ControllerRoutes.TournamentAddPlayers)]
    [HttpPost]
    public async Task<TournamentModel> AddPlayers(string id, [FromBody] List<string> players) 
    {
      var tournament = await _tournaments.FindByIdAsync(id);

      if (tournament == null)
        throw new ArgumentOutOfRangeException(nameof(id), "Invalid tournament");

      if (players == null || !players.Any())
        throw new ArgumentNullException(nameof(id), "No players");

      //todo: Create FindByIdsAsync to make one DB call
      var playerSummaries = new List<PlayerSummary>();
      foreach (var playerId in players)
      {
        var player = await _players.FindByIdAsync(playerId);
        if (player == null)
          throw new ArgumentOutOfRangeException(nameof(id), $"Invalid player: {playerId}");
        playerSummaries.Add(_mapper.Map<PlayerSummary>(player));
      }


      tournament.Players = tournament.Players.Union(playerSummaries).ToList();

      await _tournaments.ReplaceOneAsync(tournament);
      return _mapper.Map<TournamentModel>(tournament);
    }

    [Route(ControllerRoutes.TournamentCalculate)]
    [HttpPost]
    public async Task<List<FourBallModel>> Calculate(string id, int playersPerGroup = 4)
    {
      var tournament = await _tournaments.FindByIdAsync(id);

      if (tournament == null)
        throw new ArgumentOutOfRangeException(nameof(id), "Invalid tournament");

      if(!tournament.Players.Any() && !tournament.Guests.Any())
        throw new InvalidOperationException($"No players or guests loaded");

      var playerCount = tournament.Players.Count;
      var guestCount = tournament.Guests.Count;

      if (playerCount + guestCount <= playersPerGroup)
      {
        tournament.FourBalls.Add(new FourBall()
        {
          Players = tournament.Players,
          Guests = tournament.Guests
        });
        await _tournaments.ReplaceOneAsync(tournament);
      }
      else
      {
        var numberOfGroups = Math.DivRem(playerCount+guestCount, playersPerGroup, out var res);
        if (res > 0)
        {
          numberOfGroups++;
          var numberOfGuestsToAdd = (int) (playersPerGroup * res);
          for (int guestNumber = 1; guestNumber <= numberOfGuestsToAdd; guestNumber++)
          {
            tournament.Guests.Add(new Guest(){Name=$"Guest-{guestNumber}"});
          }
        }

        var fourBalls = new List<FourBall>(numberOfGroups);
        for (var sequence = 1; sequence <= numberOfGroups; sequence++)
        {
          var fourBall = new FourBall()
          {
            Sequence = sequence
          };
          while (tournament.HasPeople() && !fourBall.IsFull(playersPerGroup))
          {
            if (tournament.Players.Any())
            {
              _addPlayerToFourBall(tournament, fourBall);
              continue;;
            }

            if (tournament.Guests.Any())
            {
              _addGuestToFourBall(tournament, fourBall);
            }
          }
          fourBalls.Add(fourBall);
        }
        tournament.FourBalls = fourBalls;
      }

      return _mapper.Map<List<FourBallModel>>(tournament.FourBalls);
    }

    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
      await _tournaments.DeleteByIdAsync(id);
    }

    private async Task<Group> _validateAndGetGroup(string groupId)
    {
      var group = await _groups.FindByIdAsync(groupId);
      if (group == null)
        throw new ArgumentOutOfRangeException(nameof(groupId), $"Invalid group id: {groupId}");
      return group;
    }
    private static void _addGuestToFourBall(Tournament tournament, FourBall fourBall)
    {
      var playerIndexToMove = tournament.Guests.Count==1?
        0:
        Faker.RandomNumber.Next(0, tournament.Guests.Count - 1);
      var player = tournament.Guests[playerIndexToMove];
      tournament.Guests.RemoveAt(playerIndexToMove);
      fourBall.Guests.Add(player);
    }

    private static void _addPlayerToFourBall(Tournament tournament, FourBall fourBall)
    {
      var playerIndexToMove = tournament.Players.Count==1?
        0:
        Faker.RandomNumber.Next(0, tournament.Players.Count - 1);
      var player = tournament.Players[playerIndexToMove];
      tournament.Players.RemoveAt(playerIndexToMove);
      fourBall.Players.Add(player);
    }

  }
}
