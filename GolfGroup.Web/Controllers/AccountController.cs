using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GolfGroup.Api.Interfaces;
using GolfGroup.Api.Models;
using GolfGroup.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

namespace GolfGroup.Api.Controllers
{
  [Route(ControllerRoutes.Account)]
  [ApiController]
  [Authorize(Roles = "SystemAdmin")]
  public class AccountController: ControllerBase
  {
    private readonly IRepository<User> _users;
    private readonly IRepository<Group> _groups;
    private readonly IRepository<Player> _players;
    private readonly IMapper _mapper;
    private readonly IDatabaseSettings _databaseSettings;

    public AccountController(IRepository<User> users,
      IRepository<Group> groups,
      IRepository<Player> players,
      IOptions<DatabaseSettings> databaseSettings,
      IMapper mapper)
    {
      _users = users;
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
    [HttpPost("authenticate")]
    public async Task<ActionResult<TokenModel>> Authenticate([FromBody] AuthenticateModel model)
    {
      var user = await _login(model.UserName, model.Password);

      if (user == null)
        return BadRequest(new { message = "Username or password is incorrect" });

      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_databaseSettings.Secret);
      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new Claim[]
        {
          new Claim(ClaimTypes.Name, user.Email),
          new Claim(ClaimTypes.Email, user.Email),
          new Claim(ClaimTypes.Role, user.Role.ToString())
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
      };
      var token = tokenHandler.CreateToken(tokenDescriptor);
      var tokenString = tokenHandler.WriteToken(token);

      // return basic user info and authentication token
      return Ok(new TokenModel
      {
        Email = user.Email,
        Role = user.Role.ToString(),
        Token = tokenString
      });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserModel>> RegisterAsPlayer([FromBody] PlayerRegisterModel registerModel)
    {
      if (registerModel == null)
        return BadRequest("Invalid registration data: No data");

      var existingUser = await _users.FindOneAsync(_ => _.Email == registerModel.Email);
      if(existingUser!=null)
       return BadRequest("Invalid registration data: Email exist");

      var userModel = await Create(_mapper.Map<UserCreateModel>(registerModel));

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
      var users = _users.AsQueryable();
      var model = _mapper.Map<IList<UserModel>>(users);
      return Ok(model);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
      var user = await _users.FindByIdAsync(id);
      var model = _mapper.Map<UserModel>(user);
      return Ok(model);
    }

    [HttpPost()]
    public async Task<ActionResult<UserModel>> Create([FromBody] UserCreateModel model)
    {
      // map model to entity and set id
      var user = _mapper.Map<User>(model);

      try
      {
        user.CreatePassword(model.Password);
        await _validateAndAddGroups(model, user);

        // update user 
        await _users.InsertOneAsync(user);
        return Ok(_mapper.Map<UserModel>(user));
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
      var user = _mapper.Map<User>(model);
      user.Id = ObjectId.Parse(id);

      try
      {
        // update user 
        await _users.ReplaceOneAsync(user);
        return Ok(_mapper.Map<UserModel>(user));
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
      await _users.DeleteByIdAsync(id);
      return Ok();
    }

    private async Task<User> _login(string username, string password)
    {
      if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        return null;

      var user = await _users.FindOneAsync(_ => _.Email == username);

      // check if username exists
      if (user == null)
        return null;

      // check if password is correct
      var userPassword = Convert.FromBase64String(user.Password);
      var userPasswordSalt = Convert.FromBase64String(user.PasswordSalt);
      return !Models.User.VerifyPasswordHash(password, userPassword, userPasswordSalt) ?
        null :
        user;

      // authentication successful
    }
    private async Task _validateAndAddGroups(UserCreateModel model, User user)
    {
      if (model.Groups!=null && model.Groups.Any())
      {
        user.Groups.Clear();
        foreach (var groupId in model.Groups)
        {
          var group = await _groups.FindByIdAsync(groupId);
          if (@group == null)
            throw new ArgumentOutOfRangeException(nameof(model), $"Invalid group: {groupId}");
          user.Groups.Add(@group);
        }
      }
    }
  }
}


