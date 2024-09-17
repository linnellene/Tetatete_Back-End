using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TetaBackend.Domain.Entities.CategoryInfo;
using TetaBackend.Features.Shared.Extentions;
using TetaBackend.Features.User.Dto;
using TetaBackend.Features.User.Dto.Category;
using TetaBackend.Features.User.Enums;
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
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

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

    [SwaggerOperation(Summary = "Returns user category info if exists. If not - 404")]
    [HttpGet("categoryInfo")]
    [Authorize]
    public async Task<ActionResult> GetCategoryUserInfo()
    {
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

        var userIdGuid = new Guid(userId);

        var existingInfoType = await _userService.GetFulfilledInfoType(userIdGuid);

        if (existingInfoType is null)
        {
            return NotFound("No category info.");
        }

        switch (existingInfoType)
        {
            case CategoryType.Friends:
            {
                var info = (await _userService.GetCategoryInfo<FriendsCategoryInfoEntity>(userIdGuid))!;

                var response = new UserFriendsCategoryInfoDto
                {
                    Info = info.Info,
                    CategoryType = CategoryType.Friends
                };

                return Ok(response);
            }

            case CategoryType.Love:
            {
                var info = (await _userService.GetCategoryInfo<LoveCategoryInfoEntity>(userIdGuid))!;

                var response = new UserLoveCategoryInfoDto
                {
                    Info = info.Info,
                    GenderId = info.GenderId,
                    MaxAge = info.MaxAge,
                    MinAge = info.MinAge,
                    CategoryType = CategoryType.Love
                };

                return Ok(response);
            }

            default:
            {
                var info = (await _userService.GetCategoryInfo<WorkCategoryInfoEntity>(userIdGuid))!;

                var response = new UserWorkCategoryInfoDto()
                {
                    Info = info.Info,
                    Income = info.Income,
                    Skills = info.Skills,
                    LookingFor = info.LookingFor,
                    CategoryType = CategoryType.Work
                };

                return Ok(response);
            }
        }
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

    [SwaggerOperation(Summary = "Updates user info if exists. If not - 404")]
    [HttpPatch("info")]
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

    [SwaggerOperation(Summary =
        "Creates love user category info if not exists. If exists - overwrites, if exists by the same type - 404.")]
    [HttpPost("categoryInfo/love")]
    [Authorize]
    public async Task<ActionResult> FillLoveUserCategoryInfo([FromBody] FillLoveCategoryInfoDto dto)
    {
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

        var userIdGuid = new Guid(userId);
        var existingInfoType = await _userService.GetFulfilledInfoType(userIdGuid);

        if (existingInfoType == CategoryType.Love)
        {
            return BadRequest("Cannot insert same type.");
        }

        if (existingInfoType is not null)
        {
            await _userService.DeleteCategoryInfo(existingInfoType.Value, userIdGuid);
        }

        try
        {
            var entity = new LoveCategoryInfoEntity
            {
                UserId = userIdGuid,
                Info = dto.Info,
                GenderId = dto.GenderId,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
            };

            await _userService.FillCategoryInfo(userIdGuid, entity);

            return Created();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [SwaggerOperation(Summary =
        "Creates friends user category info if not exists. If exists - overwrites, if exists by the same type - 404.")]
    [HttpPost("categoryInfo/friends")]
    [Authorize]
    public async Task<ActionResult> FillFriendsUserCategoryInfo([FromBody] FillFriendsCategoryInfoDto dto)
    {
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

        var userIdGuid = new Guid(userId);
        var existingInfoType = await _userService.GetFulfilledInfoType(userIdGuid);

        if (existingInfoType == CategoryType.Friends)
        {
            return BadRequest("Cannot insert same type.");
        }

        if (existingInfoType is not null)
        {
            await _userService.DeleteCategoryInfo(existingInfoType.Value, userIdGuid);
        }

        try
        {
            var entity = new FriendsCategoryInfoEntity
            {
                UserId = userIdGuid,
                Info = dto.Info,
            };

            await _userService.FillCategoryInfo(userIdGuid, entity);

            return Created();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [SwaggerOperation(Summary =
        "Creates work user category info if not exists. If exists - overwrites, if exists by the same type - 404.")]
    [HttpPost("categoryInfo/work")]
    [Authorize]
    public async Task<ActionResult> FillWorkUserCategoryInfo([FromBody] FillWorkCategoryInfoDto dto)
    {
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

        var userIdGuid = new Guid(userId);
        var existingInfoType = await _userService.GetFulfilledInfoType(userIdGuid);

        if (existingInfoType == CategoryType.Work)
        {
            return BadRequest("Cannot insert same type.");
        }

        if (existingInfoType is not null)
        {
            await _userService.DeleteCategoryInfo(existingInfoType.Value, userIdGuid);
        }

        try
        {
            var entity = new WorkCategoryInfoEntity
            {
                UserId = userIdGuid,
                Info = dto.Info,
                Income = dto.Income,
                Skills = dto.Skills,
                LookingFor = dto.LookingFor,
            };

            await _userService.FillCategoryInfo(userIdGuid, entity);

            return Created();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [SwaggerOperation(Summary = "Updates user friends category info if exists. If not - 404")]
    [HttpPatch("categoryInfo/friends")]
    [Authorize]
    public async Task<ActionResult> UpdateFriendsUserCategoryInfo([FromBody] UpdateFriendsCategoryInfoDto dto)
    {
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

        var existingInfoType = await _userService.GetFulfilledInfoType(new Guid(userId));

        if (existingInfoType is null)
        {
            return NotFound("Info doesn't exist.");
        }

        try
        {
            await _userService.UpdateCategoryInfo(dto.Id, dto);

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Updates user love category info if exists. If not - 404")]
    [HttpPatch("categoryInfo/love")]
    [Authorize]
    public async Task<ActionResult> UpdateLoveUserCategoryInfo([FromBody] UpdateLoveCategoryInfoDto dto)
    {
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

        var existingInfoType = await _userService.GetFulfilledInfoType(new Guid(userId));

        if (existingInfoType is null)
        {
            return NotFound("Info doesn't exist.");
        }

        try
        {
            await _userService.UpdateCategoryInfo(dto.Id, dto);

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Updates user work category info if exists. If not - 404")]
    [HttpPatch("categoryInfo/work")]
    [Authorize]
    public async Task<ActionResult> UpdateWorkUserCategoryInfo([FromBody] UpdateWorkCategoryInfoDto dto)
    {
        var userId = HttpContext.GetUserIdFromJwt(_jwtService);

        if (userId is null)
        {
            return NotFound("No auth token provided.");
        }

        var existingInfoType = await _userService.GetFulfilledInfoType(new Guid(userId));

        if (existingInfoType is null)
        {
            return NotFound("Info doesn't exist.");
        }

        try
        {
            await _userService.UpdateCategoryInfo(dto.Id, dto);

            return Ok();
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