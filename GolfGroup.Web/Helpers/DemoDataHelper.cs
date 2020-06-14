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
    public const string DefaultAdmin = "admin@golfgroup.org";
    public const string DefaultAdminPassword = "MorningMen@Pwd1";
    public const string DefaultGroupAdmin = "groupadmin@golfgroup.org";

    public static async Task Populate(IServiceProvider serviceProvider)
    {
      await BuildGroupsAndPlayers(
        serviceProvider.GetService<IRepository<Group>>(),
        serviceProvider.GetService<IRepository<Player>>());
      await BuildUsers(
        serviceProvider.GetService<IRepository<User>>(), 
        serviceProvider.GetService<IRepository<Group>>());
    }

    private static async Task BuildUsers(IRepository<User> users, IRepository<Group> groups)
    {
      var demoGroup = await groups.FindOneAsync(_ => _.Name == DemoGroupName);
      var userGroups = new List<Group>() {demoGroup};
      var userName = DefaultAdmin;
      await _validateOrCreateUser(users, userName, null, GolfGroupRole.SystemAdmin);
      userName = DefaultGroupAdmin;
      await _validateOrCreateUser(users, userName, userGroups, GolfGroupRole.SystemAdmin);
    }

    private static async Task _validateOrCreateUser(IRepository<User> users, string userName, List<Group> groups,
      GolfGroupRole role)
    {
      var defaultAdmin = await users.FindOneAsync(_ => _.Email == userName);
      if (defaultAdmin != null)
        return;

      defaultAdmin = new User()
      {
        Email = userName,
        CreatedBy = "DemoDataHelper",
        Groups = groups,
        Role = role
      };
      defaultAdmin.CreatePassword(DefaultAdminPassword);

      await users.InsertOneAsync(defaultAdmin);
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
