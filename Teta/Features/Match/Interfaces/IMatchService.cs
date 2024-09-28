using TetaBackend.Features.Match.Dto;

namespace TetaBackend.Features.Match.Interfaces;

public interface IMatchService
{
    Task<GetMatchInfos> GetNewMatchUsers(Guid userId);
    
    Task<GetMatchInfos> GetUnansweredMatches(Guid userId);

    Task LikeUser(Guid from, Guid to);

    Task Dislike(Guid initiator, Guid receiver);
}