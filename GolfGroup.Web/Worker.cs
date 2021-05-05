using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace GolfGroup.Api
{
  public class Worker: IHostedService
  {


    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

     public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            await RegisterApplicationsAsync(scope.ServiceProvider);
            await RegisterScopesAsync(scope.ServiceProvider);

            static async Task RegisterApplicationsAsync(IServiceProvider provider)
            {
                var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

                if (await manager.FindByClientIdAsync("blazor") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "blazor",
                        ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                        ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                        DisplayName = "Blazor client application",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("fr-FR")] = "Application cliente Blazor"
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("https://localhost:44381/signout-callback-oidc")
                        },
                        RedirectUris =
                        {
                            new Uri("https://localhost:44381/signin-oidc")
                        },
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.Endpoints.Logout,
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                            OpenIddictConstants.Permissions.ResponseTypes.Code,
                            OpenIddictConstants.Permissions.Scopes.Email,
                            OpenIddictConstants.Permissions.Scopes.Profile,
                            OpenIddictConstants.Permissions.Scopes.Roles,
                            OpenIddictConstants.Permissions.Prefixes.Scope + "golf-group-api"
                        },
                        Requirements =
                        {
                            OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }

                // To test this sample with Postman, use the following settings:
                //
                // * Authorization URL: https://localhost:44395/connect/authorize
                // * Access token URL: https://localhost:44395/connect/token
                // * Client ID: postman
                // * Client secret: [blank] (not used with public clients)
                // * Scope: openid email profile roles
                // * Grant type: authorization code
                // * Request access token locally: yes
                if (await manager.FindByClientIdAsync("testclient") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "testclient",
                        ConsentType = OpenIddictConstants.ConsentTypes.Systematic,
                        DisplayName = "testclient",
                        RedirectUris =
                        {
                            new Uri("urn:testclient")
                        },
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.Endpoints.Device,
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                            OpenIddictConstants.Permissions.GrantTypes.DeviceCode,
                            OpenIddictConstants.Permissions.GrantTypes.Password,
                            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                            OpenIddictConstants.Permissions.ResponseTypes.Code,
                            OpenIddictConstants.Permissions.Scopes.Email,
                            OpenIddictConstants.Permissions.Scopes.Profile,
                            OpenIddictConstants.Permissions.Scopes.Roles
                        }
                    });
                }
            }

            static async Task RegisterScopesAsync(IServiceProvider provider)
            {
                var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

                if (await manager.FindByNameAsync("golf-group-api") is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        DisplayName = "Golf Group API access",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("fr-FR")] = "Accès à l'API de golf group"
                        },
                        Name = "golf-group-api",
                        Resources =
                        {
                            "resource_server"
                        }
                    });
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
  }
}
