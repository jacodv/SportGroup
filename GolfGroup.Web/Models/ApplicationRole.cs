using AspNetCore.Identity.Mongo.Model;

namespace GolfGroup.Api.Models
{
  public class ApplicationRole : MongoRole
  {
    public const string SystemAdminRole = "SystemAdmin";
    public const string GroupAdminRole = "GroupAdmin";
    public const string PlayerRole = "Player";

    public ApplicationRole()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }
  }
}
