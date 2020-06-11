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
  public class GroupController : ControllerBase
  {
    private readonly IRepository<Group> _repository;
    private readonly IMapper _mapper;

    public GroupController(IRepository<Group> repository, IMapper mapper)
    {
      _repository = repository;
      _mapper = mapper;
    }

    [HttpGet]
    public IEnumerable<string> Get()
    {
      return _repository.AsQueryable().Select(s=>s.Name);
    }

    [HttpGet("{id}")]
    public async Task<GroupModel> Get(string id)
    {
      return _mapper.Map<GroupModel>(await _repository.FindByIdAsync(id));
    }

    [HttpPost]
    public async Task<GroupModel> Post([FromBody] string value)
    {
      var newItem = new Group(){Name = value};
      await _repository.InsertOneAsync(newItem);
      return _mapper.Map<GroupModel>(newItem);
    }

    [HttpPut("{id}")]
    public async Task<GroupModel> Put(string id, [FromBody] string value)
    {
      var updateItem = new Group()
      {
        Id = ObjectId.Parse(id),
        Name = value
      };

      await _repository.ReplaceOneAsync(updateItem);
      return _mapper.Map<GroupModel>(updateItem);
    }

    [HttpDelete("{id}")]
    public void Delete(string id)
    {
      _repository.DeleteById(id);
    }
  }
}