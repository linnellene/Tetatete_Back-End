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

    public async Task<GetMatchInfo> GetNewMatchUser(Guid userId)
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

    public async Task<GetUnansweredMatchInfo> GetUnansweredMatches(Guid userId)
    {
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            throw new ArgumentException("Invalid user id.");
        }

        var usersToAnswer = await _dataContext.Matches
            .Include(m => m.Initiator)
            .Where(m => m.ReceiverId == userId && m.IsMatch == false)
            .Select(m => m.Initiator)
            .ToListAsync();

        if (usersToAnswer.Count == 0)
        {
            throw new ArgumentException("No unanswered matches.");
        }

        var res = new List<IMatchInfoBase>();
        var type = await _userService.GetFulfilledInfoType(usersToAnswer[0].Id);

        if (type is null)
        {
            throw new ArgumentException("Unexpected category type.");
        }

        foreach (var userToAnswer in usersToAnswer)
        {
            res.Add((await GetInfo(type.Value, userToAnswer.Id, true)).Info);
        }

        return new GetUnansweredMatchInfo
        {
            CategoryType = type.Value,
            Infos = res
        };
    }

    public async Task LikeUser(Guid from, Guid to)
    {
                
        if (from == to)
        {
            throw new ArgumentException("Cannot like yourself.");
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

    public async Task LikeUserAsAnswer(Guid initiator, Guid receiver)
    {
        var match = await _dataContext.Matches.FirstOrDefaultAsync(m =>
            m.InitiatorId == initiator && m.ReceiverId == receiver);

        if (match is null)
        {
            throw new ArgumentException("No matches with this user.");
        }

        match.IsMatch = true;

        await _dataContext.SaveChangesAsync();
    }

    public async Task DislikeUserAsAnswer(Guid initiator, Guid receiver)
    {
        var match = await _dataContext.Matches.FirstOrDefaultAsync(m =>
            m.InitiatorId == initiator && m.ReceiverId == receiver);

        if (match is null)
        {
            throw new ArgumentException("No matches with this user.");
        }

        _dataContext.Matches.Remove(match);

        await _dataContext.SaveChangesAsync();
    }

    private async Task<GetMatchInfo> GetInfo(CategoryType categoryType, Guid userId, bool unanswered = false)
    {
        IMatchInfoBase info = new FriendsMatchInfo();

        switch (categoryType)
        {
            case CategoryType.Friends:
            {
                UserEntity user;

                var usersWithIncludes = _dataContext.Users
                    .Include(u => u.UserInfo)
                    .Include(u => u.FriendsCategoryInfo)
                    .Include(u => u.ReceivedMatches)
                    .Include(u => u.InitiatedMatches);

                if (!unanswered)
                {
                    var users = await usersWithIncludes
                        .Where(u =>
                            u.Id != userId &&
                            u.FriendsCategoryInfo != null &&
                            u.UserInfo != null && (
                                !u.InitiatedMatches.Any(m => m.InitiatorId == u.Id && m.ReceiverId == userId) &&
                                !u.ReceivedMatches.Any(m => m.ReceiverId == u.Id && m.InitiatorId == userId)
                            ))
                        .ToListAsync();

                    if (users.Count == 0)
                    {
                        throw new DataException("No users with the same category.");
                    }

                    var idx = Random.Shared.Next(0, users.Count);

                    user = users[idx];
                }
                else
                {
                    user = (await usersWithIncludes.FirstOrDefaultAsync(u => u.Id == userId))!;
                }

                info = new FriendsMatchInfo
                {
                    UserId = user.Id,
                    Age = user.UserInfo!.Age,
                    Name = user.UserInfo.FullName,
                    AboutMe = user.FriendsCategoryInfo!.Info
                };

                break;
            }
            case CategoryType.Love:
            {
                UserEntity user;

                var usersWithIncludes = _dataContext.Users
                    .Include(u => u.UserInfo)
                    .Include(u => u.LoveCategoryInfo)
                    .Include(u => u.ReceivedMatches)
                    .Include(u => u.InitiatedMatches);

                if (!unanswered)
                {
                    var users = await usersWithIncludes
                        .Where(u =>
                            u.Id != userId &&
                            u.LoveCategoryInfo != null &&
                            u.UserInfo != null && (
                                !u.InitiatedMatches.Any(m => m.InitiatorId == u.Id && m.ReceiverId == userId) &&
                                !u.ReceivedMatches.Any(m => m.ReceiverId == u.Id && m.InitiatorId == userId)
                            ))
                        .ToListAsync();

                    if (users.Count == 0)
                    {
                        throw new DataException("No users with the same category.");
                    }

                    var idx = Random.Shared.Next(0, users.Count);

                    user = users[idx];
                }
                else
                {
                    user = (await usersWithIncludes.FirstOrDefaultAsync(u => u.Id == userId))!;
                }

                info = new LoveMatchInfo
                {
                    UserId = user.Id,
                    Age = user.UserInfo!.Age,
                    Name = user.UserInfo.FullName,
                    RelationshipGoals = user.LoveCategoryInfo!.Info,
                    MinAge = user.LoveCategoryInfo!.MinAge,
                    MaxAge = user.LoveCategoryInfo!.MaxAge,
                    GenderId = user.LoveCategoryInfo!.GenderId,
                };

                break;
            }
            case CategoryType.Work:
            {
                UserEntity user;

                var usersWithIncludes = _dataContext.Users
                    .Include(u => u.UserInfo)
                    .Include(u => u.WorkCategoryInfo)
                    .Include(u => u.ReceivedMatches)
                    .Include(u => u.InitiatedMatches);

                if (!unanswered)
                {
                    var users = await usersWithIncludes
                        .Where(u =>
                            u.Id != userId &&
                            u.WorkCategoryInfo != null &&
                            u.UserInfo != null && (
                                !u.InitiatedMatches.Any(m => m.InitiatorId == u.Id && m.ReceiverId == userId) &&
                                !u.ReceivedMatches.Any(m => m.ReceiverId == u.Id && m.InitiatorId == userId)
                            ))
                        .ToListAsync();

                    if (users.Count == 0)
                    {
                        throw new DataException("No users with the same category.");
                    }

                    var idx = Random.Shared.Next(0, users.Count);

                    user = users[idx];
                }
                else
                {
                    user = (await usersWithIncludes .FirstOrDefaultAsync(u => u.Id == userId))!;
                }

                info = new WorkMatchInfo
                {
                    UserId = user.Id,
                    Age = user.UserInfo!.Age,
                    Name = user.UserInfo.FullName,
                    Occupation = user.WorkCategoryInfo!.Info,
                    Income = user.WorkCategoryInfo!.Income,
                    Skills = user.WorkCategoryInfo!.Skills,
                    LookingFor = user.WorkCategoryInfo!.LookingFor,
                };

                break;
            }
        }

        return new GetMatchInfo
        {
            Info = info,
            CategoryType = categoryType
        };
    }
}