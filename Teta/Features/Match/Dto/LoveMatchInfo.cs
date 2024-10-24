using TetaBackend.Features.Match.Dto.Base;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.Match.Dto;

public class LoveMatchInfo : IMatchInfoBase
{
    public CategoryType CategoryType { get; set; } = CategoryType.Love;
    
    public string RelationshipGoals { get; set; }

    public int MinAge { get; set; }

    public int MaxAge { get; set; }
    
    public Guid GenderId { get; set; }
    
    public Guid UserId { get; set; }
    
    public List<string> ImageUrls { get; set; }

    public string Name { get; set; }
    
    public int Age { get; set; }
}