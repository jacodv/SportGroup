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
  [Route("api/[controller]")]
  [ApiController]
  public class PlayerController : ControllerBase
  {
    private readonly IRepository<Player> _playerRepository;
    private readonly IMapper _mapper;

    public PlayerController(IRepository<Player> playerRepository, IMapper mapper)
    {
      _playerRepository = playerRepository;
      _mapper = mapper;
    }

    [HttpGet]
    public IEnumerable<PlayerSummaryModel> Get()
    {
      return _mapper.Map<IEnumerable<PlayerSummaryModel>>(_playerRepository.AsQueryable());
    }

    [HttpGet("{id}", Name = "Get")]
    public async Task<PlayerModel> Get(string id)
    {
      return _mapper.Map<PlayerModel>(await _playerRepository.FindByIdAsync(id));
    }

    [HttpPost]
    public async Task<PlayerModel> Post([FromBody] PlayerCreateUpdateModel value)
    {
      var newPlayer = _mapper.Map<Player>(value);
      await _playerRepository.InsertOneAsync(newPlayer);
      return _mapper.Map<PlayerModel>(newPlayer);
    }

    [HttpPut("{id}")]
    public async Task<PlayerModel> Put(string id, [FromBody] PlayerCreateUpdateModel value)
    {
      var updatePlayer = _mapper.Map<Player>(value);
      updatePlayer.Id = ObjectId.Parse(id);

      await _playerRepository.ReplaceOneAsync(updatePlayer);
      return _mapper.Map<PlayerModel>(updatePlayer);
    }

    [HttpDelete("{id}")]
    public void Delete(string id)
    {
      _playerRepository.DeleteById(id);
    }
  }
}
