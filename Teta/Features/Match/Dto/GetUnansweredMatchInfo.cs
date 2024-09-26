using TetaBackend.Features.Match.Dto.Base;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.Match.Dto;

public class GetUnansweredMatchInfo
{
    public CategoryType CategoryType { get; set; }
    
    public List<IMatchInfoBase> Infos { get; set; }
}