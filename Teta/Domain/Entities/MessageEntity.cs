using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class MessageEntity : BaseEntity
{
    public string Content { get; set; }
    
    public Guid SenderId { get; set; }
    
    public Guid ChatId { get; set; }
    
    public UserEntity Sender { get; set; }
    
    public ChatEntity Chat { get; set; }
}