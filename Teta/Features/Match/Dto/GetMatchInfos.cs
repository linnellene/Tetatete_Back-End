using TetaBackend.Features.Match.Dto.Base;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.Match.Dto;

public class GetMatchInfos
{
    public CategoryType CategoryType { get; set; }
    
    public List<IMatchInfoBase> Info { get; set; }
}