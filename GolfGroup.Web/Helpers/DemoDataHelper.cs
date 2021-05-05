using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GolfGroup.Api.Interfaces;
using GolfGroup.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenIddict.MongoDb;
using OpenIddict.MongoDb.Models;

namespace GolfGroup.Api.Helpers
{
  public static class DemoDataHelper
  {
    public const string DemoGroupName = "MorningMen";
    public const string DefaultAdmin = "admin@golfgroup.org";
    public const string DefaultAdminPassword = "MorningMen@Pwd1";
    public const string DefaultGroupAdmin = "groupadmin@golfgroup.org";

    public static async Task SetupOpenIddictCollections(IServiceProvider provider)
    {
      var context = provider.GetRequiredService<IOpenIddictMongoDbContext>();
      var options = provider.GetRequiredService<IOptionsMonitor<OpenIddictMongoDbOptions>>().CurrentValue;
      var database = await context.GetDatabaseAsync(CancellationToken.None);
      var applications = database.GetCollection<OpenIddictMongoDbApplication>(options.ApplicationsCollectionName);
      await applications.Indexes.CreateManyAsync(new[]
      {
                new CreateIndexModel<OpenIddictMongoDbApplication>(
                    Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(application => application.ClientId),
                    new CreateIndexOptions
                    {
                        Unique = true
                    }),
                new CreateIndexModel<OpenIddictMongoDbApplication>(
                    Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(application => application.PostLogoutRedirectUris),
                    new CreateIndexOptions
                    {
                        Background = true
                    }),
                new CreateIndexModel<OpenIddictMongoDbApplication>(
                    Builders<OpenIddictMongoDbApplication>.IndexKeys.Ascending(application => application.RedirectUris),
                    new CreateIndexOptions
                    {
                        Background = true
                    })
            });
      var authorizations = database.GetCollection<OpenIddictMongoDbAuthorization>(options.AuthorizationsCollectionName);
      await authorizations.Indexes.CreateOneAsync(new CreateIndexModel<OpenIddictMongoDbAuthorization>(
          Builders<OpenIddictMongoDbAuthorization>.IndexKeys
              .Ascending(authorization => authorization.ApplicationId)
              .Ascending(authorization => authorization.Scopes)
              .Ascending(authorization => authorization.Status)
              .Ascending(authorization => authorization.Subject)
              .Ascending(authorization => authorization.Type),
          new CreateIndexOptions
          {
            Background = true
          }));
      var scopes = database.GetCollection<OpenIddictMongoDbScope>(options.ScopesCollectionName);
      await scopes.Indexes.CreateOneAsync(new CreateIndexModel<OpenIddictMongoDbScope>(
          Builders<OpenIddictMongoDbScope>.IndexKeys.Ascending(scope => scope.Name),
          new CreateIndexOptions
          {
            Unique = true
          }));
      var tokens = database.GetCollection<OpenIddictMongoDbToken>(options.TokensCollectionName);
      await tokens.Indexes.CreateManyAsync(new[]
      {
                new CreateIndexModel<OpenIddictMongoDbToken>(
                    Builders<OpenIddictMongoDbToken>.IndexKeys.Ascending(token => token.ReferenceId),
                    new CreateIndexOptions<OpenIddictMongoDbToken>
                    {
                        // Note: partial filter expressions are not supported on Azure Cosmos DB.
                        // As a workaround, the expression and the unique constraint can be removed.
                        PartialFilterExpression = Builders<OpenIddictMongoDbToken>.Filter.Exists(token => token.ReferenceId),
                        Unique = true
                    }),
                new CreateIndexModel<OpenIddictMongoDbToken>(
                    Builders<OpenIddictMongoDbToken>.IndexKeys
                        .Ascending(token => token.ApplicationId)
                        .Ascending(token => token.Status)
                        .Ascending(token => token.Subject)
                        .Ascending(token => token.Type),
                    new CreateIndexOptions
                    {
                        Background = true
                    })
            });
    }

    public static async Task SetupIdentities(IServiceProvider provider)
    {
      await _setupRoles(provider);
      await _setupUsers(provider);
    }

    private static async Task _setupUsers(IServiceProvider provider)
    {
      var userManager = provider.GetService<UserManager<ApplicationUser>>();

      var systemAdminUser = await userManager.FindByEmailAsync(DefaultAdmin);
      if (systemAdminUser != null)
        return;

      var adminUser = new ApplicationUser(DefaultAdmin)
      {
        Email = DefaultAdmin,
        IsActive = true
      };

      var result = await userManager.CreateAsync(adminUser, DefaultAdminPassword);
    }

    private static async Task _setupRoles(IServiceProvider provider)
    {
      var roleStore = provider.GetService<IRoleStore<ApplicationRole>>();
      var systemRole = await roleStore.FindByNameAsync(ApplicationRole.SystemAdminRole, default);
      if (systemRole != null)
        return;
      var result = await roleStore.CreateAsync(new ApplicationRole(ApplicationRole.SystemAdminRole), default);
      if (!result.Succeeded)
        throw new InvalidOperationException($"Failed to create role: {result}");
      result = await roleStore.CreateAsync(new ApplicationRole(ApplicationRole.GroupAdminRole), default);
      if (!result.Succeeded)
        throw new InvalidOperationException($"Failed to create role: {result}");
      result = await roleStore.CreateAsync(new ApplicationRole(ApplicationRole.PlayerRole), default);
      if (!result.Succeeded)
        throw new InvalidOperationException($"Failed to create role: {result}");
    }

    public static async Task Populate(IServiceProvider serviceProvider)
    {
      await BuildGroupsAndPlayers(
        serviceProvider.GetService<IRepository<Group>>(),
        serviceProvider.GetService<IRepository<Player>>());
      await BuildUsers(
        serviceProvider.GetService<UserManager<ApplicationUser>>(),
        serviceProvider.GetService<IRepository<Group>>());
    }

    private static async Task BuildUsers(UserManager<ApplicationUser> userManager, IRepository<Group> groups)
    {
      var demoGroup = await groups.FindOneAsync(_ => _.Name == DemoGroupName);
      var userGroups = new List<Group>() { demoGroup };
      var userName = DefaultAdmin;
      await _validateOrCreateUser(userManager, userName, null, GolfGroupRole.SystemAdmin);
      userName = DefaultGroupAdmin;
      await _validateOrCreateUser(userManager, userName, userGroups, GolfGroupRole.SystemAdmin);
    }

    private static async Task _validateOrCreateUser(UserManager<ApplicationUser> userManager, string userName, List<Group> groups,
      GolfGroupRole role)
    {
      var defaultAdmin = await userManager.FindByEmailAsync(userName);
      if (defaultAdmin != null)
        return;

      defaultAdmin = new ApplicationUser()
      {
        UserName = userName,
        Email = userName,
        Groups = groups,
        IsActive = true
      };

      var result = await userManager.CreateAsync(defaultAdmin, DefaultAdminPassword);
      if (!result.Succeeded)
        throw new InvalidOperationException(
          $"Failed to create the user: {userName}\n{string.Join(',', result.Errors)}");
      result = await userManager.AddToRoleAsync(defaultAdmin, role.ToString());
      if (!result.Succeeded)
        throw new InvalidOperationException(
          $"Failed to add role {role} to {userName}\n{string.Join(',', result.Errors)}");
    }

    private static async Task BuildGroupsAndPlayers(IRepository<Group> groups, IRepository<Player> players)
    {
      if (groups.AsQueryable().Any())
        return;

      var morningMen = new Group() { Name = DemoGroupName };
      await groups.InsertOneAsync(morningMen);

      for (var i = 0; i < 24; i++)
      {
        var player = new Player()
        {
          FirstName = Faker.Name.First(),
          LastName = Faker.Name.Last(),
          CreatedBy = "System",
          DateOfBirth = new DateTime(1973, Faker.RandomNumber.Next(1, 12), i + 1),
          Email = Faker.Internet.Email(),
          Mobile = Faker.Phone.Number(),
          Groups = new List<string>() { morningMen.Id.ToString() },
          IsEnabled = true
        };
        await players.InsertOneAsync(player);
      }
    }
  }
}
