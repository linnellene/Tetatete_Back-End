using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.User.Dto.Base;

public class UserCategoryInfoBase
{
    public Guid Id { get; set; }
    
    public CategoryType CategoryType { get; set; }   
}