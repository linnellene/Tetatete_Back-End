using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TetaBackend.Domain.Entities.CategoryInfo;
using TetaBackend.Features.User.Dto.Category;
using TetaBackend.Features.User.Enums;
using TetaBackend.Features.User.Interfaces;

namespace TetaBackend.Features.User.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoryInfoController : ControllerBase
{
    private readonly IUserService _userService;

    public CategoryInfoController(IUserService userService)
    {
        _userService = userService;
    }

    [SwaggerOperation(Summary = "Returns user category info if exists. If not - 404")]
    [HttpGet]
    public async Task<ActionResult> GetCategoryUserInfo()
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

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

    [SwaggerOperation(Summary =
        "Creates love user category info if not exists. If exists - overwrites, if exists by the same type - 404.")]
    [HttpPost("love")]
    public async Task<ActionResult> FillLoveUserCategoryInfo([FromBody] FillLoveCategoryInfoDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

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
    [HttpPost("friends")]
    public async Task<ActionResult> FillFriendsUserCategoryInfo([FromBody] FillFriendsCategoryInfoDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

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
    [HttpPost("work")]
    public async Task<ActionResult> FillWorkUserCategoryInfo([FromBody] FillWorkCategoryInfoDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

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
    [HttpPatch("friends")]
    public async Task<ActionResult> UpdateFriendsUserCategoryInfo([FromBody] UpdateFriendsCategoryInfoDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

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
    [HttpPatch("love")]
    public async Task<ActionResult> UpdateLoveUserCategoryInfo([FromBody] UpdateLoveCategoryInfoDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

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
    [HttpPatch("work")]
    public async Task<ActionResult> UpdateWorkUserCategoryInfo([FromBody] UpdateWorkCategoryInfoDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;
        
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
}