using System;
using System.Collections.Generic;
using System.Globalization;
using GolfGroup.Api.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using OpenIddict.Validation.AspNetCore;

namespace GolfGroup.Api.StartUp
{
  public static class AddOpenIddict
  {
    public static IServiceCollection SetupOpenIddict(this IServiceCollection services, IConfiguration configuration)
    {
      var databaseSettings = configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();

      
      services.AddOpenIddict()
        .AddCore(options =>
        {
          // ...
          options.UseMongoDb(mongo =>
          {
            mongo.Configure(config =>
            {
              config.Database =
                new MongoClient(databaseSettings.ConnectionString).GetDatabase(databaseSettings.DatabaseName);
            });
          });
          options.UseQuartz();
        })
        .AddServer(options =>
        {
          // Enable the token endpoint.
          options.SetTokenEndpointUris("/connect/token");

          // Enable the password and the refresh token flows.
          options
            .AllowPasswordFlow()
            .AllowRefreshTokenFlow();

          // Accept anonymous clients (i.e clients that don't send a client_id).
          options.AcceptAnonymousClients();

          // Register the signing and encryption credentials.
          options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

          // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
          options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .DisableTransportSecurityRequirement();

          //// ...
          //// Enable the authorization, device, logout, token, userinfo and verification endpoints.
          //options.SetAuthorizationEndpointUris("/connect/authorize")
          //  .SetDeviceEndpointUris("/connect/device")
          //  .SetLogoutEndpointUris("/connect/logout")
          //  .SetTokenEndpointUris("/connect/token")
          //  .SetUserinfoEndpointUris("/connect/userinfo")
          //  .SetVerificationEndpointUris("/connect/verify");

          //// Note: this sample uses the code, device code, password and refresh token flows, but you
          //// can enable the other flows if you need to support implicit or client credentials.
          //options.AllowAuthorizationCodeFlow()
          //  .AllowDeviceCodeFlow()
          //  .AllowPasswordFlow()
          //  .AllowRefreshTokenFlow();

          //// Mark the "email", "profile", "roles" and "demo_api" scopes as supported scopes.
          options.RegisterScopes(
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
            "golf-group");

          // Register the signing and encryption credentials.
          //options
          //  .AddDevelopmentEncryptionCertificate()
          //  .AddDevelopmentSigningCertificate();

          //// Force client applications to use Proof Key for Code Exchange (PKCE).
          //options.RequireProofKeyForCodeExchange();

          options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));

          //// Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
          //options.UseAspNetCore()
          //  .EnableStatusCodePagesIntegration()
          //  .EnableAuthorizationEndpointPassthrough()
          //  .EnableLogoutEndpointPassthrough()
          //  .EnableTokenEndpointPassthrough()
          //  .EnableUserinfoEndpointPassthrough()
          //  .EnableVerificationEndpointPassthrough()
          //  .DisableTransportSecurityRequirement(); // During development, you can disable the HTTPS requirement.

          //// Note: if you don't want to specify a client_id when sending
          //// a token or revocation request, uncomment the following line:
          ////
          //// options.AcceptAnonymousClients();

          //// Note: if you want to process authorization and token requests
          //// that specify non-registered scopes, uncomment the following line:
          ////
          //// options.DisableScopeValidation();

          //// Note: if you don't want to use permissions, you can disable
          //// permission enforcement by uncommenting the following lines:
          ////
          //// options.IgnoreEndpointPermissions()
          ////        .IgnoreGrantTypePermissions()
          ////        .IgnoreResponseTypePermissions()
          ////        .IgnoreScopePermissions();

          //// Note: when issuing access tokens used by third-party APIs
          //// you don't own, you can disable access token encryption:
          ////
          //// options.DisableAccessTokenEncryption();
        })
        .AddValidation(options =>
        {
          // Configure the audience accepted by this resource server.
          // The value MUST match the audience associated with the
          // "demo_api" scope, which is used by ResourceController.
          //options.AddAudiences("resource_server");

          // Import the configuration from the local OpenIddict server instance.
          options.UseLocalServer();

          // Register the ASP.NET Core host.
          options.UseAspNetCore();

          // For applications that need immediate access token or authorization
          // revocation, the database entry of the received tokens and their
          // associated authorizations can be validated for each API call.
          // Enabling these options may have a negative impact on performance.
          //
          // options.EnableAuthorizationEntryValidation();
          // options.EnableTokenEntryValidation();
        });

      // authentication
      var authenticationBuilder = services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
      });


      return services;
    }
  }

  public class DictionaryCultureInfoSerializer : IBsonSerializer<IReadOnlyDictionary<CultureInfo, string>>
  {
    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
      return Deserialize(context, args);
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args,
      IReadOnlyDictionary<CultureInfo, string> value)
    {
      context.Writer.WriteStartDocument();
      foreach (var (key, s) in value)
      {
        context.Writer.WriteString(key.Name, s);
      }

      context.Writer.WriteEndDocument();
    }

    public IReadOnlyDictionary<CultureInfo, string> Deserialize(BsonDeserializationContext context,
      BsonDeserializationArgs args)
    {
      var dictionary = new Dictionary<CultureInfo, string>();

      context.Reader.ReadStartDocument();
      while (context.Reader.ReadBsonType() != BsonType.EndOfDocument && context.Reader.State != BsonReaderState.EndOfDocument)
      {
        var key = context.Reader.ReadName();
        var value = context.Reader.ReadString();

        var cultureInfo = CultureInfo.GetCultureInfo(key);
        dictionary.Add(cultureInfo, value);
      }
      context.Reader.ReadEndDocument();

      return dictionary;
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
      Serialize(context, args, (IReadOnlyDictionary<CultureInfo, string>) value);
    }

    public Type ValueType => typeof(IReadOnlyDictionary<CultureInfo, string>);
  }
}
