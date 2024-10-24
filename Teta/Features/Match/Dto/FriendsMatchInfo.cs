using TetaBackend.Features.Match.Dto.Base;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.Match.Dto;

public class FriendsMatchInfo : IMatchInfoBase
{
    public CategoryType CategoryType { get; set; } = CategoryType.Friends;
    
    public string AboutMe { get; set; }
    
    public Guid UserId { get; set; }
    
    public List<string> ImageUrls { get; set; }

    public string Name { get; set; }
    
    public int Age { get; set; }
}