using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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

    [SwaggerOperation(Summary = "Check JWT auth access.")]
    [HttpGet]
    [Authorize]
    public ActionResult CheckAuth()
    {
        return Ok();
    }

    [SwaggerOperation(Summary = "Returns user info if exists. If not - 404.")]
    [HttpGet("info")]
    [Authorize]
    public async Task<ActionResult> GetUserInfo()
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;
        
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

    
    [SwaggerOperation(Summary = "Saves user info if not exists. If exists - 400")]
    [HttpPost("info")]
    [Authorize]
    public async Task<ActionResult> AddUserInfo([FromForm] FillUserInfoDto dto)
    {
        if (!dto.Image.ContentType.Contains("image/"))
        {
            return BadRequest("Invalid image type.");
        }

        var userId = HttpContext.Items["UserId"]?.ToString()!;

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

    [SwaggerOperation(Summary = "Updates user info if exists. If not - 404")]
    [HttpPatch("info")]
    [Authorize]
    public async Task<ActionResult> UpdateUserInfo([FromForm] UpdateUserInfoDto dto)
    {
        if (dto.Image is not null && !dto.Image.ContentType.Contains("image/"))
        {
            return BadRequest("Invalid image type.");
        }

        var userId = HttpContext.Items["UserId"]?.ToString()!;

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

    [SwaggerOperation(Summary = "Register new account.")]
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

    [SwaggerOperation(Summary = "Log into account with JWT as result.")]
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