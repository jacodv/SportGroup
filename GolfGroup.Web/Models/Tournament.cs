using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GolfGroup.Api.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace GolfGroup.Api.Models
{
  [BsonCollection("tournament")]
  public class Tournament : Document
  {
    [BsonDateTimeOptions(DateOnly = true)] 
    public DateTime Date { get; set; }
    public string Display { get; set; }
    public Group Group { get; set; }
  }

  public class TournamentModel:DocumentModel
  {
    public DateTime Date { get; set; }
    public string Display { get; set; }
    public GroupModel Group { get; set; }
  }

  public class TournamentSummaryModel
  {
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Display { get; set; }
    public string GroupName { get; set; }
  }

  public class TournamentCreateUpdateModel
  {
    private string _display;
    [Required]
    public DateTime Date { get; set; }

    public string Display
    {
      get => string.IsNullOrEmpty(_display)?$"{Date:D}":_display;
      set => _display = value;
    }

    [Required]
    public string GroupId { get; set; }
  }

  public class TournamentProfile : Profile
  {
    public TournamentProfile()
    {
      CreateMap<Tournament, TournamentModel>();
      CreateMap<Tournament, TournamentSummaryModel>()
        .ForMember(
          m => m.GroupName, 
          opt => 
            opt.MapFrom(f => f.Group.Name));

      CreateMap<TournamentCreateUpdateModel, Tournament>()
        .ForMember(
          m=>m.Group,
          opt=>
            opt.Ignore());

      CreateMap<TournamentModel, TournamentCreateUpdateModel>()
        .ForMember(
          m=>m.GroupId,
          opt=>
            opt.MapFrom(f=>f.Group.Id));
    }
  }
}