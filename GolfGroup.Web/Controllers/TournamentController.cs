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

    public TournamentController(IRepository<Tournament> tournaments, IMapper mapper, 
      IRepository<Group> groups)
    {
      _tournaments = tournaments;
      _mapper = mapper;
      _groups = groups;
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
  }
}
