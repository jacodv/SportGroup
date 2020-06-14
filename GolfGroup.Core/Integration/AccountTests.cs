using System.Data.Common;
using System.Threading.Tasks;
using AutoMapper;
using FizzWare.NBuilder;
using FluentAssertions;
using GolfGroup.Api.Helpers;
using GolfGroup.Api.Models;
using Xunit;

namespace GolfGroup.Api.Tests.Integration
{
  public class AccountTests: IntegrationTestBase
  {
    private IMapper _mapper;

    public AccountTests() : base("api/account")
    {
      var config = new MapperConfiguration(cfg => {
        cfg.AddProfile<PlayerProfile>();
      });

      _mapper = config.CreateMapper();
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

    [Fact]
    public async Task AddUser_GivenNewUser_ShouldAddUser()
    {
      // arrange
      var newUser = Builder<UserCreateModel>
        .CreateNew()
        .With(_=>_.Role = GolfGroupRole.GroupAdmin.ToString())
        .Build();

      // action
      var createdUser = await _post<UserModel>(BaseUrl, newUser);

      // assert
      createdUser.Email.Should().Be(newUser.Email);
      createdUser.Role.Should().Be(newUser.Role);

      await _deleteAndValidate(createdUser.Id, BaseUrl);
    }

    [Fact]
    public async Task UpdateUser_GivenNewUser_ShouldAddUser()
    {
      // arrange
      var newUser = Builder<UserCreateModel>
        .CreateNew()
        .With(_ => _.Role = GolfGroupRole.GroupAdmin.ToString())
        .Build();
      var createdUser = await _post<UserModel>(BaseUrl, newUser);

      // action
      createdUser.Role = GolfGroupRole.Player.ToString();
      createdUser.Email = "SomeNewEmail@Domain.com";
      var updatedUser = await _put<UserModel>($"{BaseUrl}/{createdUser.Id}", createdUser);

      // assert
      updatedUser.Email.Should().Be(createdUser.Email);
      updatedUser.Role.Should().Be(createdUser.Role);
      updatedUser.Groups.Should().BeNullOrEmpty();

      await _deleteAndValidate(createdUser.Id, BaseUrl);
    }

    [Fact]
    public async Task RegisterAsPlayer_GivenNewPlayer_ShouldCreateUserAndPlayer()
    {
      // arrange
      var newPlayerRegistration = _mapper.Map<PlayerRegisterModel>(PlayerTests.CreatePlayerCreateUpdateModel());
      newPlayerRegistration.Password = $"Some123@1Pwd";

      // action
      var newUserModel = await _post<UserModel>($"{BaseUrl}/register", newPlayerRegistration, true);
      var playerModel = await _get<PlayerModel>($"api/player/email/{newPlayerRegistration.Email}");

      // assert
      newUserModel.Email.Should().Be(newPlayerRegistration.Email);
      playerModel.Email.Should().Be(newPlayerRegistration.Email);
      await _deleteAndValidate(newUserModel.Id, BaseUrl);
    }
  }
}
