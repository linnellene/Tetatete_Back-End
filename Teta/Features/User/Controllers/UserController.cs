using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TetaBackend.Features.User.Dto;
using TetaBackend.Features.User.Dto.OAuth;
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
        return Ok(new CheckAuthResponse
        {
            IsAuth = true,
        });
    }

    [SwaggerOperation(Summary = "Gets all genders.")]
    [HttpGet("genders")]
    [Authorize]
    public async Task<ActionResult> GetGenders()
    {
        var genders = await _userService.GetAllGenders();

        return Ok(genders.Select(g => new GenderDto
        {
            Id = g.Id,
            Name = g.Name,
        }));
    }

    [SwaggerOperation(Summary = "Gets all locations.")]
    [HttpGet("locations")]
    [Authorize]
    public async Task<ActionResult> GetLocations()
    {
        var locations = await _userService.GetAllLocations();

        return Ok(locations.Select(l => new LocationDto
        {
            Id = l.Id,
            City = l.City,
            Country = l.Country
        }));
    }

    [SwaggerOperation(Summary = "Gets all languages.")]
    [HttpGet("languages")]
    [Authorize]
    public async Task<ActionResult> GetLanguages()
    {
        var languages = await _userService.GetAllLanguages();

        return Ok(languages.Select(l => new LanguageDto
        {
            Id = l.Id,
            Name = l.Name
        }));
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
            ImageUrls = userInfo.Images.Select(i => i.Url).ToList(),
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
        if (!dto.Images.All(f => f.ContentType.StartsWith("image/")))
        {
            return BadRequest("Invalid images type.");
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
                ImageUrls = info.Images.Select(i => i.Url).ToList(),
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
        if (dto.Images is not null && !dto.Images.All(f => f.ContentType.StartsWith("image/")))
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
                ImageUrls = newInfo.Images.Select(i => i.Url).ToList(),
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
    public async Task<ActionResult> RegisterUser([FromBody] RegisterDto dto)
    {
        try
        {
            await _userService.CreateUser(dto.Email, dto.Phone, dto.Password);

            return Created();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Send email to a specified email token to update password.")]
    [HttpPost("forgotPassword")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            await _userService.SendForgotPasswordEmail(dto.Email);

            return Created();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Update user password.")]
    [HttpPatch("updatePassword")]
    [Authorize]
    public async Task<ActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            await _userService.UpdatePassword(new Guid(userId), dto.NewPassword);

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Log into account with JWT as result.")]
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var authUser = await _userService.Authenticate(dto.Password, dto.Email, dto.Phone);

            if (authUser is null)
            {
                return NotFound("Invalid credentials to log in.");
            }

            var jwt = _jwtService.GenerateToken(authUser.Id, authUser.Email);

            return Ok(new LoginResponseDto
            {
                Token = jwt
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("googleLogin")]
    public ActionResult LoginGoogle()
    {
        return Ok(new RequestOauthLoginResponseDto
        {
            Url = _userService.GenerateGoogleLoginLink()
        });
    }

    [HttpPost("googleResponse")]
    public async Task<ActionResult> GoogleResponseAsync(OauthResponseDto dto)
    {
        try
        {
            return Ok(new LoginResponseDto
            {
                Token = await _userService.AuthorizeUserFromGoogleAsync(dto.Code)
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("facebookLogin")]
    public ActionResult LoginFacebook()
    {
        return Ok(new RequestOauthLoginResponseDto
        {
            Url = _userService.GenerateFacebookLoginLink()
        });
    }

    [HttpPost("facebookResponse")]
    public async Task<ActionResult> FacebookResponseAsync(OauthResponseDto dto)
    {
        try
        {
            return Ok(new LoginResponseDto
            {
                Token = await _userService.AuthorizeUserFromFacebookAsync(dto.Code)
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}