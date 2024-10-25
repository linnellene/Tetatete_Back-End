using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TetaBackend.Features.Chat.Dto;
using TetaBackend.Features.Chat.Interfaces;

namespace TetaBackend.Features.Chat.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [SwaggerOperation(Summary = "Gets user chat rooms.")]
    [HttpGet("chatRooms")]
    public async Task<ActionResult> GetUserChatRooms()
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            return Ok(await _chatService.GetUserChatsWithLastMessages(new Guid(userId)));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(Summary = "Gets chat messages.")]
    [HttpGet("messages")]
    public async Task<ActionResult> GetChatMessages([FromQuery] GetChatMessagesDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;
        var userIdGuid = new Guid(userId);

        try
        {
            var messages = await _chatService.GetChatMessages(userIdGuid, dto.ChatId);

            return Ok(messages.Select(m => new MessageDto
            {
                ChatId = m.ChatId,
                Content = m.Content,
                SentByUser = m.SenderId == userIdGuid,
                Timestamp = m.CreatedAt
            }));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [SwaggerOperation(Summary = "Leave chat.")]
    [HttpPost("leave")]
    public async Task<ActionResult> LeaveChat([FromQuery] LeaveOrJoinChatDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            await _chatService.LeaveChat(new Guid(userId), dto.ChatId);

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [SwaggerOperation(Summary = "Join chat.")]
    [HttpPost("join")]
    public async Task<ActionResult> Join([FromQuery] LeaveOrJoinChatDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            await _chatService.JoinChat(new Guid(userId), dto.ChatId);

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}