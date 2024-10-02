using System.ComponentModel.DataAnnotations.Schema;
using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class ChatEntity : BaseEntity
{
    public string Name { get; set; }
    
    public Guid UserAId { get; set; }
    
    public Guid UserBId { get; set; }
    
    public bool UserALeft { get; set; } = false;
    
    public bool UserBLeft { get; set; } = false;
    
    [ForeignKey("UserAId")]
    public UserEntity UserA { get; set; }
    
    [ForeignKey("UserBId")]
    public UserEntity UserB { get; set; }
    
    public ICollection<MessageEntity> Messages { get; set; }
}