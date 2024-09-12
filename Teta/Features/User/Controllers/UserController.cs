using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TetaBackend.Features.User.Dto;
using TetaBackend.Features.User.Interfaces;

namespace TetaBackend.Features.User.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public UserController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }
    
    // get: api/User - checks if has an access by JWT
    [HttpGet]
    [Authorize]
    public ActionResult CheckAuth()
    {
        return Ok();
    }

    // post: api/User/register - registers new account
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody] AuthDto dto)
    {
        try
        {
            await _userService.ValidateRegisterParameters(dto.Username, dto.Password);

            var isSaved = await _userService.CreateUser(dto.Username, dto.Password);

            return isSaved ? Created() : BadRequest("Unexpected error.");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // post: api/User/login - returns JWT to authenticate
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] AuthDto dto)
    {
        try
        {
            await _userService.ValidateRegisterParameters(dto.Username, dto.Password, true);

            var authUser = await _userService.Authenticate(dto.Username, dto.Password);

            if (authUser is null)
            {
                return Unauthorized("Invalid credentials to log in.");
            }

            var jwt = _jwtService.GenerateToken(authUser.Id);

            return Ok(jwt);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}

