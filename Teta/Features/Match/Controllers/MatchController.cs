using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TetaBackend.Features.Match.Dto;
using TetaBackend.Features.Match.Interfaces;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.Match.Controllers;

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
                CategoryType.Friends => Ok(data.Infos.Select(i => i as FriendsMatchInfo)),
                CategoryType.Work => Ok(data.Infos.Select(i => i as WorkMatchInfo)),
                _ => Ok(data.Infos.Select(i => i as LoveMatchInfo))
            };
            
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Gets random user to match")]
    [HttpGet("new")]
    public async Task<ActionResult> GetRandomUser()
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            var data = await _matchService.GetNewMatchUser(new Guid(userId));

            return data.CategoryType switch
            {
                CategoryType.Friends => Ok(data.Info as FriendsMatchInfo),
                CategoryType.Work => Ok(data.Info as WorkMatchInfo),
                _ => Ok(data.Info as LoveMatchInfo)
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

    [SwaggerOperation(Summary = "Likes user as an answer.")]
    [HttpPost("likeInAnswer")]
    public async Task<ActionResult> LikeUserAsAnswer([FromQuery] Guid responseUserId)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            await _matchService.LikeUserAsAnswer(responseUserId, new Guid(userId));

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Dislikes user as an answer.")]
    [HttpDelete("dislike")]
    public async Task<ActionResult> DislikeUser([FromQuery] Guid responseUserId)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            await _matchService.DislikeUserAsAnswer(responseUserId, new Guid(userId));

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}