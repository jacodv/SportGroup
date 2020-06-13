using System;
using System.Collections.Generic;
using AutoMapper;

namespace GolfGroup.Api.Models
{
  public class FourBall : Document
  {
    public FourBall()
    {
      Players = new List<PlayerSummary>();
      Guests = new List<Guest>();
    }
    public int Sequence { get; set; } = 1;
    public Tee Tee { get; set; }
    public DateTime? Time { get; set; }
    public List<PlayerSummary> Players { get; set; }
    public List<Guest> Guests { get; set; }

    public bool IsFull(int playerInGroup)
    {
      var playerCount = Players.Count + Guests.Count;
      if (playerCount > playerInGroup)
        throw new InvalidOperationException("To many people");
      return playerCount == playerInGroup;
    }
  }

  public class FourBallModel
  {
    public string Id { get; set; }
    public int Sequence { get; set; }
    public Tee Tee { get; set; }
    public DateTime? Time { get; set; }
    public List<PlayerSummaryModel> Players { get; set; }
    public List<GuestModel> Guests { get; set; }
  }

  public class FourBallProfile : Profile
  {
    public FourBallProfile()
    {
      CreateMap<FourBall, FourBallModel>();
    }
  }


  public enum Tee
  {
    NotSet,
    Front,
    Back
  }
}
