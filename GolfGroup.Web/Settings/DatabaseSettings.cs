using GolfGroup.Api.Interfaces;

namespace GolfGroup.Api.Settings
{
  public class DatabaseSettings : IDatabaseSettings
  {
    public string DatabaseName { get; set; }="GolfGroup";
    public string ConnectionString { get; set; } = "mongodb://localhost";
  }
}
