using AspNetCore.Identity.Mongo;
using GolfGroup.Api.Models;
using GolfGroup.Api.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using OpenIddict.Abstractions;

namespace GolfGroup.Api.StartUp
{
  public static class AddIdentity
  {
    public static IServiceCollection ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
    {
      var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();

      // Configure Identity to use the same JWT claims as OpenIddict instead
      // of the legacy WS-Federation claims it uses by default (ClaimTypes),
      // which saves you from doing the mapping in your authorization controller.

      // Register the Identity services.
      services
        .AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole>(identity =>
        {
          identity.Password.RequiredLength = 8;
          // other options
        }, mongo =>
        {
          mongo.ConnectionString = $"{databaseSettings.ConnectionString}/{databaseSettings.DatabaseName}";
        })
        .AddDefaultTokenProviders();

      try
      {
        BsonSerializer.RegisterSerializer(new DictionaryCultureInfoSerializer());
      }
      catch {}

      services.Configure<IdentityOptions>(options =>
      {
        options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
        options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
        options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
      });

      return services;
    }
  }
}
