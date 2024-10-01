using System.ComponentModel.DataAnnotations;
using TetaBackend.Domain.Enums;
using TetaBackend.Features.Interfaces;

namespace TetaBackend.Features.User.Dto.Category;

public class FillWorkCategoryInfoDto : ICategory
{
    public string Info { get; set; }
    
    public int Income { get; set; }
    
    [Range(0, 1, ErrorMessage = "Invalid LookingFor value. LookingFor must be between 0 and 1.")]
    public WorkCategoryLookingFor LookingFor { get; set; }
    
    public string Skills { get; set; }
}