using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TetaBackend.Features.Match.Dto;
using TetaBackend.Features.Match.Interfaces;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.Match.Controllers;

[Route("api/[controller]")]
[Authorize]
public class MatchController : Controller
{
    private readonly IMatchService _matchService;

    public MatchController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [SwaggerOperation(Summary = "Gets unanswered matches")]
    [HttpGet("unanswered")]
    public async Task<ActionResult> GetUnansweredMatches()
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            var data = await _matchService.GetUnansweredMatches(new Guid(userId));

            return data.CategoryType switch
            {
                CategoryType.Friends => Ok(data.Info.Select(i => i as FriendsMatchInfo)),
                CategoryType.Work => Ok(data.Info.Select(i => i as WorkMatchInfo)),
                _ => Ok(data.Info.Select(i => i as LoveMatchInfo))
            };
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Gets up to 5 random users to match")]
    [HttpGet("new")]
    public async Task<ActionResult> GetRandomUser()
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            var data = await _matchService.GetNewMatchUsers(new Guid(userId));

            return data.CategoryType switch
            {
                CategoryType.Friends => Ok(data.Info.Select(i => i as FriendsMatchInfo)),
                CategoryType.Work => Ok(data.Info.Select(i => i as WorkMatchInfo)),
                _ => Ok(data.Info.Select(i => i as LoveMatchInfo))
            };
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [SwaggerOperation(Summary = "Gets user matches")]
    [HttpGet("existing")]
    public async Task<ActionResult> GetUserMatches()
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            var data = await _matchService.GetUserMatches(new Guid(userId));

            return data.CategoryType switch
            {
                CategoryType.Friends => Ok(data.Info.Select(i => i as FriendsMatchInfo)),
                CategoryType.Work => Ok(data.Info.Select(i => i as WorkMatchInfo)),
                _ => Ok(data.Info.Select(i => i as LoveMatchInfo))
            };
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Likes user.")]
    [HttpPost("like")]
    public async Task<ActionResult> LikeUser([FromQuery] Guid userToLikeId)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            await _matchService.LikeUser(new Guid(userId), userToLikeId);

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Dislikes user.")]
    [HttpDelete("dislike")]
    public async Task<ActionResult> DislikeUser([FromQuery] Guid responseUserId)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            await _matchService.Dislike(responseUserId, new Guid(userId));

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}