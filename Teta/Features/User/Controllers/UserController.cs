using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TetaBackend.Features.Shared.Extentions;
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

    // get: api/User/info - returns user info if exists
    [HttpGet("info")]
    [Authorize]
    public async Task<ActionResult> GetUserInfo()
    {
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);
        
        var userInfo = await _userService.GetUserInfo(new Guid(userId));
        
        if (userInfo is null)
        {
            return NotFound("No info.");
        }

        var responseDto = new UserInfoDto
        {
            About = userInfo.About,
            ImageUrl = userInfo.ImageUrl,
            Age = userInfo.Age,
            FullName = userInfo.FullName,
            GenderId = userInfo.GenderId,
            LanguageIds = userInfo.UserInfoLanguages.Select(ui => ui.LanguageId).ToList(),
            LocationId = userInfo.LocationId,
            PlaceOfBirthId = userInfo.PlaceOfBirthId
        };

        return Ok(responseDto);
    }

    // post: api/User/info - saves user info if not exists
    [HttpPost("info")]
    [Authorize]
    public async Task<ActionResult> AddUserInfo([FromForm] FillUserInfoDto dto)
    {
        if (!dto.Image.ContentType.Contains("image/"))
        {
            return BadRequest("Invalid image type.");
        }

        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

        var existingInfo = await _userService.GetUserInfo(new Guid(userId));

        if (existingInfo is not null)
        {
            return BadRequest("Info is already fulfilled.");
        }

        try
        {
            var info = await _userService.FillInformation(new Guid(userId), dto);

            var responseDto = new UserInfoDto
            {
                About = info.About,
                ImageUrl = info.ImageUrl,
                Age = info.Age,
                FullName = info.FullName,
                GenderId = info.GenderId,
                LanguageIds = info.UserInfoLanguages.Select(ui => ui.LanguageId).ToList(),
                LocationId = info.LocationId,
                PlaceOfBirthId = info.PlaceOfBirthId
            };

            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    // put: api/User/info - updates user info if exists
    [HttpPut("info")]
    [Authorize]
    public async Task<ActionResult> UpdateUserInfo([FromForm] UpdateUserInfoDto dto)
    {
        if (dto.Image is not null && !dto.Image.ContentType.Contains("image/"))
        {
            return BadRequest("Invalid image type.");
        }
        
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

        var existingInfo = await _userService.GetUserInfo(new Guid(userId));

        if (existingInfo is null)
        {
            return BadRequest("Info not exists.");
        }

        try
        {
            var newInfo = await _userService.UpdateInformation(new Guid(userId), dto);
            
            var responseDto = new UserInfoDto
            {
                About = newInfo.About,
                ImageUrl = newInfo.ImageUrl,
                Age = newInfo.Age,
                FullName = newInfo.FullName,
                GenderId = newInfo.GenderId,
                LanguageIds = newInfo.UserInfoLanguages.Select(ui => ui.LanguageId).ToList(),
                LocationId = newInfo.LocationId,
                PlaceOfBirthId = newInfo.PlaceOfBirthId
            };

            return Ok(responseDto);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
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