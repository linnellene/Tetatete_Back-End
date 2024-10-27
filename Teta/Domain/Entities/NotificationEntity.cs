using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class NotificationEntity : BaseEntity
{
    public string Message { get; set; }
    
    public Guid UserId { get; set; }

    public bool IsRead { get; set; } = false;
    
    public UserEntity User { get; set; }
}