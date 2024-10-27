namespace TetaBackend.Features.Notifications.Dto;

public class ReadNotificationsDto
{
    public IEnumerable<Guid> Ids { get; set; }
}