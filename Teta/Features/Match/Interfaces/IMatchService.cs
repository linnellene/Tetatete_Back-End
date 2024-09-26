using TetaBackend.Features.Match.Dto;

namespace TetaBackend.Features.Match.Interfaces;

public interface IMatchService
{
    Task<GetMatchInfo> GetNewMatchUser(Guid userId);
    
    Task<GetUnansweredMatchInfo> GetUnansweredMatches(Guid userId);

    Task LikeUser(Guid from, Guid to);

    Task LikeUserAsAnswer(Guid initiator, Guid receiver);
    
    Task DislikeUserAsAnswer(Guid initiator, Guid receiver);
}