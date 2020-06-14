using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GolfGroup.Api.Interfaces;
using GolfGroup.Api.Models;
using GolfGroup.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IMapper _mapper;
    private readonly IDatabaseSettings _databaseSettings;

    public AccountController(IRepository<User> users, 
      IOptions<DatabaseSettings> databaseSettings,
      IMapper mapper)
    {
      _users = users;
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UserCreateUpdateModel model)
    {
      // map model to entity and set id
      var user = _mapper.Map<User>(model);
      user.Id = ObjectId.Parse(id);
      //todo hash password

      try
      {
        // update user 
        await _users.ReplaceOneAsync(user);
        return Ok();
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
  }
}


