using TetaBackend.Features.Interfaces;

namespace TetaBackend.Features.User.Dto.Category;

public class FillLoveCategoryInfoDto : ICategory
{
    public string Info { get; set; }

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public Guid GenderId { get; set; }
}