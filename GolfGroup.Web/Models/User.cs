using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using GolfGroup.Api.Data;

namespace GolfGroup.Api.Models
{
  ///
  /// https://github.com/cornflourblue/aspnet-core-3-registration-login-api
  /// https://fullstackmark.com/post/21/user-authentication-and-identity-with-angular-aspnet-core-and-identityserver
  /// 
  [BsonCollection("user")]
  public class User : Document
  {
    public string Email { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }

    public GolfGroupRole Role { get; set; }

    public List<Group> Groups { get; set; }
    public bool IsActive { get; set; }

    public void CreatePassword(string password)
    {
      CreatePasswordHash(password, out var hash, out var salt);
      Password = Convert.ToBase64String(hash);
      PasswordSalt = Convert.ToBase64String(salt);
    }

    public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
      if (password == null) 
        throw new ArgumentNullException(nameof(password));
      if (string.IsNullOrWhiteSpace(password)) 
        throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
      if (storedHash.Length != 64) 
        throw new ArgumentException("Invalid length of password hash (64 bytes expected).", nameof(storedHash));
      if (storedSalt.Length != 128) 
        throw new ArgumentException("Invalid length of password salt (128 bytes expected).", nameof(storedHash));

      using (var hmac = new HMACSHA512(storedSalt))
      {
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        if (computedHash.Where((t, i) => t != storedHash[i]).Any())
        {
          return false;
        }
      }

      return true;
    }

    public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
      if (password == null) 
        throw new ArgumentNullException("password");
      if (string.IsNullOrWhiteSpace(password)) 
        throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

      using var hmac = new HMACSHA512();
      passwordSalt = hmac.Key;
      passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
  }

  public class UserModel
  {
    public string Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public List<GroupModel> Groups { get; set; }
    public bool IsActive { get; set; }
  }

  public class UserCreateUpdateModel
  {
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    public List<string> Groups { get; set; }
    public bool IsActive { get; set; }
  }

  public class TokenModel
  {
    public string Email { get; set; }
    public string Role { get; set; }
    public string Token { get; set; }
  }

  public class UserProfile : Profile
  {
    public UserProfile()
    {
      CreateMap<User, UserModel>();
      CreateMap<UserCreateUpdateModel, User>()
        .ForMember(m => m.Id,
          opt =>
            opt.Ignore())
        .ForMember(m => m.Groups,
          opt =>
            opt.Ignore());

    }
  }

  public enum GolfGroupRole
  {
    Player,
    GroupAdmin,
    SystemAdmin
  }
}
