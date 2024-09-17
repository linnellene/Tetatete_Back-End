using TetaBackend.Features.Interfaces;

namespace TetaBackend.Features.User.Dto.Base;

public class UpdateCategoryInfoBase : ICategory
{
    public Guid Id { get; set; }
    
    public string? Info { get; set; }
}