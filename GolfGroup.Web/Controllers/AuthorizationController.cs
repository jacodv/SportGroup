using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using GolfGroup.Api.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace GolfGroup.Api.Controllers
{
  [ApiController]
  public class AuthorizationController : ControllerBase
  {
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AuthorizationController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
      _signInManager = signInManager;
      _userManager = userManager;
      _roleManager = roleManager;
    }

    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
      var request = HttpContext.GetOpenIddictServerRequest();
      if (request.IsPasswordGrantType())
      {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
          var properties = new AuthenticationProperties(new Dictionary<string, string>
          {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                  "The username/password couple is invalid."
          });

          return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Validate the username/password parameters and ensure the account is not locked out.
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
          var properties = new AuthenticationProperties(new Dictionary<string, string>
          {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                  "The username/password couple is invalid."
          });

          return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Create a new ClaimsPrincipal containing the claims that
        // will be used to create an id_token, a token or a code.
        var principal = await _signInManager.CreateUserPrincipalAsync(user);

        // Set the list of scopes granted to the client application.
        // Note: the offline_access scope must be granted
        // to allow OpenIddict to return a refresh token.
        principal.SetScopes(new[]
        {
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    "golf-group"
                }.Intersect(request.GetScopes()));

        foreach (var claim in principal.Claims)
        {
          claim.SetDestinations(GetDestinations(claim, principal));
        }

        var roles = new List<string>();
        foreach (var roleId in user.Roles)
        {
          roles.Add(await _roleManager.GetRoleNameAsync(new ApplicationRole() { Id = ObjectId.Parse(roleId) }));
        }


        var props = new AuthenticationProperties(
          items: new Dictionary<string, string>(),
          parameters: new Dictionary<string, object>
          {
            ["Email"] = user.Email,
            ["Roles"] = roles.ToArray(),
          });


        return SignIn(principal, props, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      }

      else if (request.IsRefreshTokenGrantType())
      {
        // Retrieve the claims principal stored in the refresh token.
        var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Retrieve the user profile corresponding to the refresh token.
        // Note: if you want to automatically invalidate the refresh token
        // when the user password/roles change, use the following line instead:
        // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
        var user = await _userManager.GetUserAsync(info.Principal);
        if (user == null)
        {
          var properties = new AuthenticationProperties(new Dictionary<string, string>
          {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The refresh token is no longer valid."
          });

          return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Ensure the user is still allowed to sign in.
        if (!await _signInManager.CanSignInAsync(user))
        {
          var properties = new AuthenticationProperties(new Dictionary<string, string>
          {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
          });

          return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Create a new ClaimsPrincipal containing the claims that
        // will be used to create an id_token, a token or a code.
        var principal = await _signInManager.CreateUserPrincipalAsync(user);

        foreach (var claim in principal.Claims)
        {
          claim.SetDestinations(GetDestinations(claim, principal));
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      }

      throw new NotImplementedException("The specified grant type is not implemented.");
    }

    private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
      // Note: by default, claims are NOT automatically included in the access and identity tokens.
      // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
      // whether they should be included in access tokens, in identity tokens or in both.

      switch (claim.Type)
      {
        case OpenIddictConstants.Claims.Name:
          yield return OpenIddictConstants.Destinations.AccessToken;

          if (principal.HasScope(OpenIddictConstants.Permissions.Scopes.Profile))
            yield return OpenIddictConstants.Destinations.IdentityToken;

          yield break;

        case OpenIddictConstants.Claims.Email:
          yield return OpenIddictConstants.Destinations.AccessToken;

          if (principal.HasScope(OpenIddictConstants.Permissions.Scopes.Email))
            yield return OpenIddictConstants.Destinations.IdentityToken;

          yield break;

        case OpenIddictConstants.Claims.Role:
          yield return OpenIddictConstants.Destinations.AccessToken;

          if (principal.HasScope(OpenIddictConstants.Permissions.Scopes.Roles))
            yield return OpenIddictConstants.Destinations.IdentityToken;

          yield break;

        // Never include the security stamp in the access and identity tokens, as it's a secret value.
        case "AspNet.Identity.SecurityStamp": yield break;

        default:
          yield return OpenIddictConstants.Destinations.AccessToken;
          yield break;
      }
    }
  }
}
