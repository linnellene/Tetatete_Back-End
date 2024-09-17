using TetaBackend.Domain.Enums;
using TetaBackend.Features.Interfaces;

namespace TetaBackend.Features.User.Dto.Category;

public class FillWorkCategoryInfoDto : ICategory
{
    public string Info { get; set; }
    
    public int Income { get; set; }
    
    public WorkCategoryLookingFor LookingFor { get; set; }
    
    public string Skills { get; set; }
}