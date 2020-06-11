using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GolfGroup.Api.Data;

namespace GolfGroup.Api.Models
{
  [BsonCollection("group")]
  public class Group: Document
  {
    public string Name { get; set; }
  }

  public class GroupModel
  {
    public string Id { get; set; }
    public string Name { get; set; }
  }

  public class GroupProfile : Profile
  {
    public GroupProfile()
    {
      CreateMap<Group, GroupModel>();
    }
  }
}
