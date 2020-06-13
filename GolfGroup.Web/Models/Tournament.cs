using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GolfGroup.Api.Data;
using MongoDB.Bson.Serialization.Attributes;

namespace GolfGroup.Api.Models
{
  [BsonCollection("tournament")]
  public class Tournament : Document
  {
    public Tournament()
    {
      Players = new List<PlayerSummary>();
      Guests = new List<Guest>();
      FourBalls = new List<FourBall>();
    }
    [BsonDateTimeOptions(DateOnly = true)]
    public DateTime Date { get; set; }
    public string Display { get; set; }
    public Group Group { get; set; }
    public List<PlayerSummary> Players { get; set; }
    public List<Guest> Guests { get; set; }
    public List<FourBall> FourBalls { get; set; }

    public bool HasPeople()
    {
      return (Players.Count + Guests.Count) > 0;
    }
  }

  public class TournamentModel : DocumentModel
  {
    public DateTime Date { get; set; }
    public string Display { get; set; }
    public GroupModel Group { get; set; }
    public List<PlayerSummaryModel> Players { get; set; }
    public List<GuestModel> Guests { get; set; }
    public List<FourBallModel> FourBalls { get; set; }
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
      get => string.IsNullOrEmpty(_display) ? $"{Date:D}" : _display;
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
          m => m.Group,
          opt =>
            opt.Ignore());

      CreateMap<TournamentModel, TournamentCreateUpdateModel>()
        .ForMember(
          m => m.GroupId,
          opt =>
            opt.MapFrom(f => f.Group.Id));
    }
  }
}