using System.Collections.Generic;

namespace GolfGroup.Api.Models
{
  public class AuthenticateModel
  {
    public string GrantType { get; set; } = "password";
    public string ClientId { get; set; } = "testclient";
    public string UserName { get; set; }
    public string Password { get; set; }

    public Dictionary<string, string> GetFormData()
    {
      return new()
      {
        {"grant_type", GrantType},
        {"client_id", ClientId},
        {"username", UserName},
        {"password", Password},
      };
    }
  }
}
