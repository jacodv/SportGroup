using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using GolfGroup.Api.Models;
using GolfGroup.Api.Tests.Helpers;
using Newtonsoft.Json;
using Xunit;

namespace GolfGroup.Api.Tests.Integration
{
  public class GroupTests : IntegrationTestBase
  {
    private const string _baseUrl = "/api/group";

    [Fact]
    public async Task GetGroups_GivenNoArguments_ShouldReturnPlayers()
    {
      //Action
      var playersResponse = await _client.GetStringAsync(_baseUrl);

      //Assert
      playersResponse.Should().Contain("[");
    }

    [Fact]
    public async Task FullCycle_GivenValidGroup_ShouldAddUpdateAndRemoveGroup()
    {
      // arrange
      var newGroup = "NewGroup";

      // Create action
      var inserted = await _evaluateReponse<GroupModel>(await _client.PostAsync(_baseUrl, new JsonContent(newGroup)));
      inserted.Id.Should().NotBeEmpty();
      inserted.Name.Should().Be(newGroup);

      // Update action
      inserted.Name = "UpdatedGroup";
      var updated = await _evaluateReponse<GroupModel>(await _client.PutAsync($"{_baseUrl}/{inserted.Id}", new JsonContent(inserted.Name)));
      updated.Name.Should().Be(inserted.Name);
      var get = await _evaluateReponse<GroupModel>(await _client.GetAsync($"{_baseUrl}/{inserted.Id}"));
      get.Name.Should().Be(inserted.Name);

      // Delete action
      var deleteResponse = await _client.DeleteAsync($"{_baseUrl}/{inserted.Id}");
      deleteResponse.IsSuccessStatusCode.Should().BeTrue();
      var getResponse = await _client.GetAsync($"{_baseUrl}/{inserted.Id}");
      getResponse.IsSuccessStatusCode.Should().BeTrue();
      getResponse.Content.ReadAsStringAsync().Result.Should().BeNullOrEmpty();
    }

    private async Task<T> _evaluateReponse<T>(HttpResponseMessage response)
    {
      if (response.IsSuccessStatusCode)
      {
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
      }
      throw new InvalidOperationException($"Failed: {response.StatusCode}:\n{await response.Content.ReadAsStringAsync()}");
    }
  }
}