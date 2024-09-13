using System.ComponentModel.DataAnnotations;

namespace TetaBackend.Domain.Entities.Base;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
}