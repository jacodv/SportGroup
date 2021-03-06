using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GolfGroup.Api.Data;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GolfGroup.Api.Models
{
  [BsonCollection("player")]
  public class Player : Document
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NickName { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    [BsonDateTimeOptions(DateOnly = true)]
    public DateTime DateOfBirth { get; set; }

    public List<string> Groups { get; set; }

    public bool IsEnabled { get; set; } = true;
  }

  public class PlayerSummary
  {
    public ObjectId Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NickName { get; set; }
    public DateTime DateOfBirth { get; set; }

    #region Equality members

    protected bool Equals(PlayerSummary other)
    {
      return Equals(Id, other.Id);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((PlayerSummary)obj);
    }

    public override int GetHashCode()
    {
      return (Id != null ? Id.GetHashCode() : 0);
    }

    #endregion
  }

  public class PlayerProfile : Profile
  {
    public PlayerProfile()
    {
      CreateMap<Player, PlayerSummary>();
      CreateMap<Player, PlayerModel>();
      CreateMap<Player, PlayerSummaryModel>();

      CreateMap<PlayerSummary, PlayerSummaryModel>();
      CreateMap<PlayerCreateUpdateModel, Player>();
      CreateMap<PlayerCreateUpdateModel, PlayerRegisterModel>();

      CreateMap<PlayerRegisterModel, Player>();
    }
  }

  public class PlayerModel: DocumentModel
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NickName { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public DateTime DateOfBirth { get; set; }

    public bool IsEnabled { get; set; } = true;
  }

  public class PlayerSummaryModel
  {
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NickName { get; set; }
    public DateTime DateOfBirth { get; set; }
  }

  public class PlayerCreateUpdateModel
  {
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string NickName { get; set; }
   
    private string _email;

    [Required]
    [EmailAddress]
    public string Email
    {
      get => _email;
      set => _email = value?.ToLower();
    }
    public string Mobile { get; set; }
    public DateTime DateOfBirth { get; set; }
  }

  public class PlayerRegisterModel : PlayerCreateUpdateModel
  {
    [Required]
    public string Password { get; set; }
  }
}