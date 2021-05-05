using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo.Model;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;

namespace GolfGroup.Api.Models
{
  public class ApplicationUser : MongoUser
  {
    public ApplicationUser()
    {
    }

    public ApplicationUser(string email) : base(email)
    {

    }

    public List<Group> Groups { get; set; }
    public bool IsActive { get; set; }
  }

  public class UserModel
  {
    public string Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public List<GroupModel> Groups { get; set; }
    public bool IsActive { get; set; }
  }

  public class UserCreateModel : UserUpdateModel
  {
    public string Password { get; set; }
  }
  public class UserUpdateModel
  {
    public string Email { get; set; }
    public string Role { get; set; }
    public List<string> Groups { get; set; }
    public bool IsActive { get; set; }
  }


  public class TokenModel
  {
    public string Email { get; set; }
    public string[] Roles { get; set; }
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
  }

  public class UserProfile : Profile
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserProfile()
    {
      CreateMap<ApplicationUser, UserModel>();

      CreateMap<UserUpdateModel, ApplicationUser>()
        .ForMember(m => m.Id,
          opt =>
            opt.Ignore())
        .ForMember(m => m.Groups,
          opt =>
            opt.Ignore());
      CreateMap<UserCreateModel, ApplicationUser>()
        .ForMember(m => m.Email,
          opt => opt.MapFrom(x => x.Email))
        .ForMember(m => m.Id,
          opt =>
            opt.Ignore())
        .ForMember(m => m.Groups,
          opt =>
            opt.Ignore());


      CreateMap<PlayerRegisterModel, ApplicationUser>();
      CreateMap<PlayerRegisterModel, UserCreateModel>();
    }
  }

  public enum GolfGroupRole
  {
    Player,
    GroupAdmin,
    SystemAdmin
  }
}
