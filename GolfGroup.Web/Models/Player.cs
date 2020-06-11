﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GolfGroup.Api.Data;
using GolfGroup.Api.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson;

namespace GolfGroup.Api.Models
{
  [BsonCollection("player")]
  public class Player : Document
  {
    public Player()
    {
      Id = ObjectId.GenerateNewId();
    }
    public string CreatedBy { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NickName { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public DateTime DateOfBirth { get; set; }

    public List<string> Groups { get; set; }

    public bool IsEnabled { get; set; } = true;
  }

  public class PlayerProfile : Profile
  {
    public PlayerProfile()
    {
      CreateMap<Player, PlayerModel>();
      CreateMap<Player, PlayerSummaryModel>();

      CreateMap<PlayerCreateUpdateModel, Player>();
    }
  } 

  public class PlayerModel
  {
    public string Id { get; set; }

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
    [Required]
    public string Email { get; set; }
    public string Mobile { get; set; }
    public DateTime DateOfBirth { get; set; }
  }
}