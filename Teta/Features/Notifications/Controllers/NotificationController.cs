using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TetaBackend.Features.Notifications.Dto;
using TetaBackend.Features.Notifications.Interfaces;

namespace TetaBackend.Features.Notifications;

[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult> GetLatestNotifications()
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            return Ok((await _notificationService.GetLatestUserNotifications(new Guid(userId))).Select(n =>
                new NotificationDto
                {
                    Id = n.Id, Message = n.Message
                }));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult> ReadNotifications([FromBody] ReadNotificationsDto dto)
    {
        var userId = HttpContext.Items["UserId"]?.ToString()!;

        try
        {
            await _notificationService.ReadNotifications(new Guid(userId), dto.Ids);

            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}