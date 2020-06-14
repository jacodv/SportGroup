namespace GolfGroup.Api.Controllers
{
  public class ControllerRoutes
  {
    public const string Player = "api/player";
    public const string PlayerForGroup = "group/{id}";
    public const string PlayerEmail = "email/{email}";

    public const string Group = "api/group";

    public const string Tournament = "api/tournament";
    public const string TournamentAddPlayer = "{id}/player/{playerId}";
    public const string TournamentAddPlayers = "{id}/players";
    public const string TournamentAddGuest = "{id}/guest";
    public const string TournamentCalculate = "{id}/calculate";
    
    public const string Account = "api/account";
  }
}
