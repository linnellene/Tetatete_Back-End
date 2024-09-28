using TetaBackend.Features.Match.Dto.Base;

namespace TetaBackend.Features.Match.Dto;

public class LoveMatchInfo : IMatchInfoBase
{
    public string RelationshipGoals { get; set; }

    public int MinAge { get; set; }

    public int MaxAge { get; set; }
    
    public Guid GenderId { get; set; }
    
    public Guid UserId { get; set; }
    
    public List<string> ImageUrls { get; set; }

    public string Name { get; set; }
    
    public int Age { get; set; }
}