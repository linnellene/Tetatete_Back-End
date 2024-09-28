using TetaBackend.Domain.Enums;
using TetaBackend.Features.Match.Dto.Base;

namespace TetaBackend.Features.Match.Dto;

public class WorkMatchInfo : IMatchInfoBase
{
    public string Occupation { get; set; }
    
    public int Income { get; set; }
    
    public WorkCategoryLookingFor LookingFor { get; set; }
    
    public string Skills { get; set; }
    
    public Guid UserId { get; set; }
    
    public List<string> ImageUrls { get; set; }

    public string Name { get; set; }
    
    public int Age { get; set; }
    
}