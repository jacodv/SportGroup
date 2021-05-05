using System.Threading.Tasks;
using FluentAssertions;
using GolfGroup.Api.Controllers;
using GolfGroup.Api.Models;
using GolfGroup.Api.Tests.Helpers;
using Xunit;

namespace GolfGroup.Api.Tests.Integration
{
  public class GroupTests : IntegrationTestBase
  {
    public GroupTests():base(ControllerRoutes.Group)
    {
      
    }
    [Fact]
    public async Task GetGroups_GivenNoArguments_ShouldReturnPlayers()
    {
      //Action
      var playersResponse = await _client.GetStringAsync(BaseUrl);

      //Assert
      playersResponse.Should().Contain("[");
    }

    [Fact]
    public async Task FullCycle_GivenValidGroup_ShouldAddUpdateAndRemoveGroup()
    {
      // arrange
      var newGroup = "NewGroup";

      // Create action
      var inserted = await _evaluateResponse<GroupModel>(await _client.PostAsync(BaseUrl, new JsonContent(newGroup)));
      inserted.Id.Should().NotBeEmpty();
      inserted.Name.Should().Be(newGroup);

      // Update action
      inserted.Name = "UpdatedGroup";
      var updated = await _evaluateResponse<GroupModel>(await _client.PutAsync($"{BaseUrl}/{inserted.Id}", new JsonContent(inserted.Name)));
      updated.Name.Should().Be(inserted.Name);
      var get = await _evaluateResponse<GroupModel>(await _client.GetAsync($"{BaseUrl}/{inserted.Id}"));
      get.Name.Should().Be(inserted.Name);

      // Delete action
      await _deleteAndValidate(inserted.Id, BaseUrl);
    }
  }
}