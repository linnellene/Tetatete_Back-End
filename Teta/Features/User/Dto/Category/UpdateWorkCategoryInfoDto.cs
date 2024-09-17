using TetaBackend.Domain.Enums;
using TetaBackend.Features.User.Dto.Base;

namespace TetaBackend.Features.User.Dto.Category;

public class UpdateWorkCategoryInfoDto : UpdateCategoryInfoBase
{
    public int? Income { get; set; }
    
    public WorkCategoryLookingFor? LookingFor { get; set; }
    
    public string? Skills { get; set; }
}