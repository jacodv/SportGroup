using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace GolfGroup.Api.Models
{
  public class Guest: Document
  {
    public string Name { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
  }

  public class GuestModel
  {
    public string Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
  }

  public class GuestCreateUpdateModel
  {
    [Required]
    public string Name { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
  }

  public class GuestProfile : Profile
  {
    public GuestProfile()
    {
      CreateMap<Guest, GuestModel>()
        .ReverseMap();

      CreateMap<GuestCreateUpdateModel, Guest>();
    }
  }
}
