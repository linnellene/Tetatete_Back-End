using System.Data;
using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.Features.Match.Dto;
using TetaBackend.Features.Match.Dto.Base;
using TetaBackend.Features.Match.Interfaces;
using TetaBackend.Features.User.Enums;
using TetaBackend.Features.User.Interfaces;

namespace TetaBackend.Features.Match.Services;

public class MatchService : IMatchService
{
    private readonly DataContext _dataContext;
    private readonly IUserService _userService;

    public MatchService(DataContext dataContext, IUserService userService)
    {
        _dataContext = dataContext;
        _userService = userService;
    }

    public async Task<GetMatchInfos> GetNewMatchUsers(Guid userId)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            throw new ArgumentException("Invalid user id.");
        }

        var type = await _userService.GetFulfilledInfoType(userId);

        if (type is null)
        {
            throw new ArgumentException("Unexpected category type.");
        }

        return await GetInfo(type.Value, userId);
    }

    public async Task<GetMatchInfos> GetUnansweredMatches(Guid userId)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            throw new ArgumentException("Invalid user id.");
        }

        var userToAnswer = await _dataContext.Matches
            .Include(m => m.Initiator)
            .Where(m => m.ReceiverId == userId && m.IsMatch == false)
            .Select(m => m.Initiator)
            .FirstOrDefaultAsync();

        if (userToAnswer is null)
        {
            throw new ArgumentException("No unanswered matches.");
        }

        var type = await _userService.GetFulfilledInfoType(userToAnswer.Id);

        if (type is null)
        {
            throw new ArgumentException("Unexpected category type.");
        }

        return await GetInfo(type.Value, userId, true);
    }

    public async Task LikeUser(Guid from, Guid to)
    {
        if (from == to)
        {
            throw new ArgumentException("Cannot like yourself.");
        }

        var typeFrom = await _userService.GetFulfilledInfoType(from);
        var typeTo = await _userService.GetFulfilledInfoType(to);

        if (typeFrom != typeTo)
        {
            throw new ArgumentException("Different categories.");
        }

        if (await _dataContext.Matches.AnyAsync(m => m.InitiatorId == from && m.ReceiverId == to))
        {
            throw new ArgumentException("Already liked.");
        }

        var existingMatch =
            await _dataContext.Matches.FirstOrDefaultAsync(m => m.ReceiverId == from && m.InitiatorId == to);

        if (existingMatch is not null)
        {
            existingMatch.IsMatch = true;

            await _dataContext.SaveChangesAsync();

            return;
        }

        var match = new MatchEntity
        {
            InitiatorId = from,
            ReceiverId = to,
            IsMatch = false
        };

        await _dataContext.Matches.AddAsync(match);

        await _dataContext.SaveChangesAsync();
    }

    public async Task Dislike(Guid initiator, Guid receiver)
    {
        var match = await _dataContext.Matches.FirstOrDefaultAsync(m =>
            m.InitiatorId == initiator && m.ReceiverId == receiver);

        if (match is null)
        {
            throw new ArgumentException("No matches with this user.");
        }

        if (match.IsMatch)
        {
            throw new ArgumentException("Already liked.");
        }

        _dataContext.Matches.Remove(match);

        await _dataContext.SaveChangesAsync();
    }

    private async Task<GetMatchInfos> GetInfo(CategoryType categoryType, Guid userId, bool unanswered = false)
    {
        var infos = new List<IMatchInfoBase>();

        switch (categoryType)
        {
            case CategoryType.Friends:
            {
                List<UserEntity> users;

                if (!unanswered)
                {
                    users = await _dataContext.Users
                        .Include(u => u.UserInfo)
                        .Include(u => u.FriendsCategoryInfo)
                        .Include(u => u.ReceivedMatches)
                        .Include(u => u.InitiatedMatches)
                        .Where(u =>
                            u.Id != userId &&
                            u.FriendsCategoryInfo != null &&
                            u.UserInfo != null && (
                                !u.InitiatedMatches.Any(m => m.InitiatorId == u.Id && m.ReceiverId == userId) &&
                                !u.ReceivedMatches.Any(m => m.ReceiverId == u.Id && m.InitiatorId == userId)
                            ))
                        .OrderBy(x => Guid.NewGuid())
                        .Take(5)
                        .ToListAsync();

                    if (users.Count == 0)
                    {
                        throw new DataException("No users with the same category.");
                    }
                }
                else
                {
                    users = await _dataContext.Matches
                        .Include(m => m.Initiator)
                        .ThenInclude(i => i.UserInfo)
                        .Include(m => m.Initiator)
                        .ThenInclude(i => i.FriendsCategoryInfo)
                        .Where(m => m.ReceiverId == userId && m.IsMatch == false)
                        .Select(m => m.Initiator)
                        .ToListAsync();
                }

                infos = users.Select(u => new FriendsMatchInfo
                    {
                        UserId = u.Id,
                        Age = u.UserInfo!.Age,
                        Name = u.UserInfo!.FullName,
                        AboutMe = u.FriendsCategoryInfo!.Info
                    })
                    .Cast<IMatchInfoBase>()
                    .ToList();

                break;
            }
            case CategoryType.Love:
            {
                List<UserEntity> users;

                if (!unanswered)
                {
                    users = await _dataContext.Users
                        .Include(u => u.UserInfo)
                        .Include(u => u.LoveCategoryInfo)
                        .Include(u => u.ReceivedMatches)
                        .Include(u => u.InitiatedMatches)
                        .Where(u =>
                            u.Id != userId &&
                            u.LoveCategoryInfo != null &&
                            u.UserInfo != null && (
                                !u.InitiatedMatches.Any(m => m.InitiatorId == u.Id && m.ReceiverId == userId) &&
                                !u.ReceivedMatches.Any(m => m.ReceiverId == u.Id && m.InitiatorId == userId)
                            ))
                        .OrderBy(x => Guid.NewGuid())
                        .Take(5)
                        .ToListAsync();

                    if (users.Count == 0)
                    {
                        throw new DataException("No users with the same category.");
                    }
                }
                else
                {
                    users = await _dataContext.Matches
                        .Include(m => m.Initiator)
                        .ThenInclude(i => i.UserInfo)
                        .Include(m => m.Initiator)
                        .ThenInclude(i => i.LoveCategoryInfo)
                        .Where(m => m.ReceiverId == userId && m.IsMatch == false)
                        .Select(m => m.Initiator)
                        .ToListAsync();
                }

                infos = users.Select(u => new LoveMatchInfo
                    {
                        UserId = u.Id,
                        Age = u.UserInfo!.Age,
                        Name = u.UserInfo.FullName,
                        RelationshipGoals = u.LoveCategoryInfo!.Info,
                        MinAge = u.LoveCategoryInfo!.MinAge,
                        MaxAge = u.LoveCategoryInfo!.MaxAge,
                        GenderId = u.LoveCategoryInfo!.GenderId,
                    })
                    .Cast<IMatchInfoBase>()
                    .ToList();

                break;
            }
            case CategoryType.Work:
            {
                List<UserEntity> users;

                if (!unanswered)
                {
                    users = await _dataContext.Users
                        .Include(u => u.UserInfo)
                        .Include(u => u.WorkCategoryInfo)
                        .Include(u => u.ReceivedMatches)
                        .Include(u => u.InitiatedMatches)
                        .Where(u =>
                            u.Id != userId &&
                            u.WorkCategoryInfo != null &&
                            u.UserInfo != null && (
                                !u.InitiatedMatches.Any(m => m.InitiatorId == u.Id && m.ReceiverId == userId) &&
                                !u.ReceivedMatches.Any(m => m.ReceiverId == u.Id && m.InitiatorId == userId)
                            ))
                        .OrderBy(x => Guid.NewGuid())
                        .Take(5)
                        .ToListAsync();

                    if (users.Count == 0)
                    {
                        throw new DataException("No users with the same category.");
                    }
                }
                else
                {
                    users = await _dataContext.Matches
                        .Include(m => m.Initiator)
                        .ThenInclude(i => i.UserInfo)
                        .Include(m => m.Initiator)
                        .ThenInclude(i => i.WorkCategoryInfo)
                        .Where(m => m.ReceiverId == userId && m.IsMatch == false)
                        .Select(m => m.Initiator)
                        .ToListAsync();
                }

                infos = users.Select(u => new WorkMatchInfo
                    {
                        UserId = u.Id,
                        Age = u.UserInfo!.Age,
                        Name = u.UserInfo.FullName,
                        Occupation = u.WorkCategoryInfo!.Info,
                        Income = u.WorkCategoryInfo!.Income,
                        Skills = u.WorkCategoryInfo!.Skills,
                        LookingFor = u.WorkCategoryInfo!.LookingFor,
                    })
                    .Cast<IMatchInfoBase>()
                    .ToList();

                break;
            }
        }

        return new GetMatchInfos
        {
            Info = infos,
            CategoryType = categoryType
        };
    }
}