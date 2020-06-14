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
  [Route(ControllerRoutes.Player)]
  [ApiController]
  public class PlayerController : ControllerBase
  {
    private readonly IRepository<Player> _repository;
    private readonly IMapper _mapper;

    public PlayerController(IRepository<Player> repository, IMapper mapper)
    {
      _repository = repository;
      _mapper = mapper;
    }

    [HttpGet]
    public IEnumerable<PlayerSummaryModel> Get()
    {
      return _mapper.Map<IEnumerable<PlayerSummaryModel>>(_repository.AsQueryable().ToList());
    }

    [Route(ControllerRoutes.PlayerForGroup)]
    [HttpGet]
    public IEnumerable<PlayerSummaryModel> GetForGroup(string id)
    {
      return _mapper.Map<IEnumerable<PlayerSummaryModel>>(
        _repository.AsQueryable().Where(_=>_.Groups.Contains(id)));
    }

    [HttpGet("{id}")]
    public async Task<PlayerModel> Get(string id)
    {
      return _mapper.Map<PlayerModel>(await _repository.FindByIdAsync(id));
    }

    [HttpPost]
    public async Task<PlayerModel> Post([FromBody] PlayerCreateUpdateModel value)
    {
      var newPlayer = _mapper.Map<Player>(value);
      await _repository.InsertOneAsync(newPlayer);
      return _mapper.Map<PlayerModel>(newPlayer);
    }

    [HttpPut("{id}")]
    public async Task<PlayerModel> Put(string id, [FromBody] PlayerCreateUpdateModel value)
    {
      var updatePlayer = _mapper.Map<Player>(value);
      updatePlayer.Id = ObjectId.Parse(id);

      await _repository.ReplaceOneAsync(updatePlayer);
      return _mapper.Map<PlayerModel>(updatePlayer);
    }

    [HttpDelete("{id}")]
    public void Delete(string id)
    {
      _repository.DeleteById(id);
    }
  }
}
