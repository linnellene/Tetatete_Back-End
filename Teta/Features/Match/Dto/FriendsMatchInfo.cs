using TetaBackend.Features.Match.Dto.Base;

namespace TetaBackend.Features.Match.Dto;

public class FriendsMatchInfo : IMatchInfoBase
{
    public string AboutMe { get; set; }
    
    public Guid UserId { get; set; }
    
    public string Name { get; set; }
    
    public int Age { get; set; }
}