using GolfGroup.Api.Interfaces;

namespace GolfGroup.Api.Settings
{
  public class DatabaseSettings : IDatabaseSettings
  {
    public string DatabaseName { get; set; } = "GolfGroup";
    public string ConnectionString { get; set; } = "mongodb://localhost";
    public string Secret { get; set; } = "G0lfGr0up$3cret!";
  }
}
