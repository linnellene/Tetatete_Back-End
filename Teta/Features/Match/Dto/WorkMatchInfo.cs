using TetaBackend.Domain.Enums;
using TetaBackend.Features.Match.Dto.Base;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.Match.Dto;

public class WorkMatchInfo : IMatchInfoBase
{
    public CategoryType CategoryType { get; set; } = CategoryType.Work;
    
    public string Occupation { get; set; }
    
    public int Income { get; set; }
    
    public WorkCategoryLookingFor LookingFor { get; set; }
    
    public string Skills { get; set; }
    
    public Guid UserId { get; set; }
    
    public List<string> ImageUrls { get; set; }

    public string Name { get; set; }
    
    public int Age { get; set; }
    
}