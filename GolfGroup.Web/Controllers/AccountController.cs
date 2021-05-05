using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GolfGroup.Api.Interfaces;
using GolfGroup.Api.Models;
using GolfGroup.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace GolfGroup.Api.Controllers
{
  [Route(ControllerRoutes.Account)]
  [ApiController]
  [Authorize()]
  public class AccountController: ControllerBase
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    private readonly IRepository<Group> _groups;
    private readonly IRepository<Player> _players;
    private readonly IMapper _mapper;
    private readonly IDatabaseSettings _databaseSettings;

    public AccountController( 
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager,
      RoleManager<ApplicationRole> roleManager,
      IRepository<Group> groups,
      IRepository<Player> players,
      IOptions<DatabaseSettings> databaseSettings,
      IMapper mapper)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _groups = groups;
      _players = players;
      _mapper = mapper;
      _databaseSettings = databaseSettings.Value;
    }

    [HttpGet]
    [AllowAnonymous]
    public string Get()
    {
      return User.Identity.IsAuthenticated?
        $"Logged in as: {User.Identity.Name}":
        "Anonymous";
    }


    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserModel>> RegisterAsPlayer([FromBody] PlayerRegisterModel registerModel)
    {
      if (registerModel == null)
        return BadRequest("Invalid registration data: No data");

      var existingUser = await _userManager.FindByEmailAsync(registerModel.Email);
      if(existingUser!=null)
       return BadRequest("Invalid registration data: Email exist");

      var userCreateModel = _mapper.Map<UserCreateModel>(registerModel);
      userCreateModel.Role = GolfGroupRole.Player.ToString();
      var userModel = await Create(userCreateModel);

      var existingPlayer = await _players.FindOneAsync(_ => _.Email == registerModel.Email);
      if (existingPlayer != null)
        return userModel;

      await _players.InsertOneAsync(_mapper.Map<Player>(registerModel));
      return userModel;
    }

    [Route("all")]
    [HttpGet]
    public IActionResult GetAll()
    {
      var model = _mapper.Map<IList<UserModel>>(_userManager.Users);
      return Ok(model);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
      var user = await _userManager.FindByIdAsync(id);
      var model = _mapper.Map<UserModel>(user);
      return Ok(model);
    }

    [HttpPost()]
    public async Task<ActionResult<UserModel>> Create([FromBody] UserCreateModel model)
    {
      // map model to entity and set id
      var user = _mapper.Map<ApplicationUser>(model);
      user.Email = model.Email;

      try
      {
        await _validateAndAddGroups(model, user);

        // update user 
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
          return BadRequest(result.Errors);

        result = await _userManager.AddToRoleAsync(user, model.Role);
        if (!result.Succeeded)
          return BadRequest(result.Errors);


        var userModel = _mapper.Map<UserModel>(user);
        var roles = new List<string>();
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var roleId in user.Roles)
        {
          roles.Add(await _roleManager.GetRoleNameAsync(new ApplicationRole() { Id = ObjectId.Parse(roleId) }));
        }

        userModel.Role = roles.FirstOrDefault();
        return Ok(userModel);
      }
      catch (Exception ex)
      {
        // return error message if there was an exception
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserModel>> Update(string id, [FromBody] UserUpdateModel model)
    {
      // map model to entity and set id
      

      try
      {
        var existingUser = await _userManager.FindByIdAsync(id);
        if (existingUser == null)
          return BadRequest("Invalid UserId");

        existingUser.IsActive = model.IsActive;
        existingUser.Email = model.Email;
        //existingUser.Groups = model.Groups;

        // update user 
        var result = await _userManager.UpdateAsync(existingUser);
        if (!result.Succeeded)
          return BadRequest(result.Errors);

        await _userManager.RemoveFromRolesAsync(existingUser, existingUser.Roles);
        await _userManager.AddToRoleAsync(existingUser, model.Role);

        return Ok(_mapper.Map<UserModel>(existingUser));
      }
      catch (Exception ex)
      {
        // return error message if there was an exception
        return BadRequest(new { message = ex.Message });
      }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
      var existingUser = await _userManager.FindByIdAsync(id);
      if (existingUser == null)
        throw new ArgumentOutOfRangeException($"Invalid user id");

      var result = await _userManager.DeleteAsync(existingUser);
      if (!result.Succeeded)
        return BadRequest(result.Errors);

      return Ok();
    }

    private async Task _validateAndAddGroups(UserCreateModel model, ApplicationUser applicationUser)
    {
      if (model.Groups!=null && model.Groups.Any())
      {
        applicationUser.Groups.Clear();
        foreach (var groupId in model.Groups)
        {
          var group = await _groups.FindByIdAsync(groupId);
          if (@group == null)
            throw new ArgumentOutOfRangeException(nameof(model), $"Invalid group: {groupId}");
          applicationUser.Groups.Add(@group);
        }
      }
    }
  }
}


