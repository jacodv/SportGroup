using System.Threading.Tasks;
using FluentAssertions;
using GolfGroup.Api.Helpers;
using GolfGroup.Api.Models;
using Xunit;

namespace GolfGroup.Api.Tests.Integration
{
  public class AccountTests: IntegrationTestBase
  {
    public AccountTests() : base("api/account")
    {
    }

    [Fact]
    public async Task GetAccount_GivenNoAnonymous_ShouldReturnAnonymous()
    {
      //Action
      var anonResponse = await _anonClient.GetStringAsync(BaseUrl);

      //Assert
      anonResponse.Should().Contain("Anonymous");
    }

    [Fact]
    public async Task GetAccount_GivenAuth_ShouldReturnLoggedInUser()
    {
      //Action
      var authResponse = await _client.GetStringAsync(BaseUrl);

      //Assert
      authResponse.Should().Contain(DemoDataHelper.DefaultAdmin);
    }

  }
}
