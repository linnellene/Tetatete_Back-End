using TetaBackend.Features.Match.Dto.Base;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.Match.Dto;

public class GetMatchInfo
{
    public CategoryType CategoryType { get; set; }
    
    public IMatchInfoBase Info { get; set; }
}