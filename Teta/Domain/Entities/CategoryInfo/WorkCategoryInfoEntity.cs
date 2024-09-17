using System.ComponentModel.DataAnnotations;
using TetaBackend.Domain.Entities.Base;
using TetaBackend.Domain.Enums;
using TetaBackend.Features.Interfaces;

namespace TetaBackend.Domain.Entities.CategoryInfo;

public class WorkCategoryInfoEntity : BaseEntity, ICategory
{
    [MaxLength(120)]
    public string Info { get; set; }
    
    public int Income { get; set; }
    
    public WorkCategoryLookingFor LookingFor { get; set; }
    
    public string Skills { get; set; }
    
    public Guid UserId { get; set; }

    public UserEntity User { get; set; }
}