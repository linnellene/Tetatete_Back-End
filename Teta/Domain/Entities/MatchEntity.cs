using System.ComponentModel.DataAnnotations.Schema;
using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class MatchEntity : BaseEntity
{
    public Guid InitiatorId { get; set; }
    
    public Guid ReceiverId { get; set; }
    
    public bool IsMatch { get; set; }
    
    [ForeignKey("InitiatorId")]
    public UserEntity Initiator { get; set; }
    
    [ForeignKey("ReceiverId")]
    public UserEntity Receiver { get; set; }
}