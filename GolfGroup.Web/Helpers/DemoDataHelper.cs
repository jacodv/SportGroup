using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GolfGroup.Api.Interfaces;
using GolfGroup.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GolfGroup.Api.Helpers
{
  public static class DemoDataHelper
  {
    public const string DemoGroupName = "MorningMen";

    public static async Task Populate(IServiceProvider serviceProvider)
    {
      await BuildGroupsAndPlayers(
        serviceProvider.GetService<IRepository<Group>>(),
        serviceProvider.GetService<IRepository<Player>>());
    }

    private static async Task BuildGroupsAndPlayers(IRepository<Group> groups, IRepository<Player> players)
    {
      if (groups.AsQueryable().Any())
        return;

      var morningMen = new Group() {Name = DemoGroupName};
      await groups.InsertOneAsync(morningMen);

      for (var i = 0; i < 24; i++)
      {
        var player = new Player()
        {
          FirstName = Faker.Name.First(),
          LastName = Faker.Name.Last(),
          CreatedBy = "System",
          DateOfBirth = new DateTime(1973, Faker.RandomNumber.Next(1,12),i+1),
          Email = Faker.Internet.Email(),
          Mobile = Faker.Phone.Number(),
          Groups = new List<string>() { morningMen.Id.ToString()},
          IsEnabled = true
        };
        await players.InsertOneAsync(player);
      }
    }
  }
}
