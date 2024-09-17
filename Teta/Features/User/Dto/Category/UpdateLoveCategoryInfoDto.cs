using TetaBackend.Features.User.Dto.Base;

namespace TetaBackend.Features.User.Dto.Category;

public class UpdateLoveCategoryInfoDto : UpdateCategoryInfoBase
{
    public int? MinAge { get; set; }

    public int? MaxAge { get; set; }

    public Guid? GenderId { get; set; }
}