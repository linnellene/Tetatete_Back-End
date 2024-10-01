using System.Data;
using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.Features.Match.Dto;
using TetaBackend.Features.Match.Dto.Base;
using TetaBackend.Features.Match.Enums;
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

    public async Task<GetMatchInfos> GetUserMatches(Guid userId)
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

        return await GetInfo(type.Value, userId, GetMatchType.Existing);
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

        return await GetInfo(type.Value, userId, GetMatchType.Unanswered);
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
            await _dataContext.Matches
                .Include(m => m.Receiver)
                .ThenInclude(r => r.UserInfo)
                .Include(m => m.Initiator)
                .ThenInclude(i => i.UserInfo)
                .FirstOrDefaultAsync(m => m.ReceiverId == from && m.InitiatorId == to);

        if (existingMatch is not null)
        {
            if (!existingMatch.Receiver.IsStripeSubscriptionPaid || !existingMatch.Initiator.IsStripeSubscriptionPaid)
            {
                throw new ArgumentException("One of the users hasn't paid his subscription!");
            }

            existingMatch.IsMatch = true;

            var newChat = new ChatEntity
            {
                UserAId = existingMatch.InitiatorId,
                UserBId = existingMatch.ReceiverId,
                Name =
                    $"Chat between {existingMatch.Initiator.UserInfo!.FullName} and {existingMatch.Receiver.UserInfo!.FullName}"
            };

            await _dataContext.Chats.AddAsync(newChat);
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
        var match = await _dataContext.Matches
            .Include(matchEntity => matchEntity.Receiver)
            .Include(matchEntity => matchEntity.Initiator)
            .FirstOrDefaultAsync(m =>
                m.InitiatorId == initiator && m.ReceiverId == receiver);

        if (match is null)
        {
            throw new ArgumentException("No matches with this user.");
        }

        if (match.IsMatch)
        {
            throw new ArgumentException("Already liked.");
        }

        if (!match.Receiver.IsStripeSubscriptionPaid || !match.Initiator.IsStripeSubscriptionPaid)
        {
            throw new ArgumentException("One of the users hasn't paid his subscription!");
        }

        _dataContext.Matches.Remove(match);

        await _dataContext.SaveChangesAsync();
    }

    private async Task<GetMatchInfos> GetInfo(CategoryType categoryType, Guid userId,
        GetMatchType type = GetMatchType.New)
    {
        var infos = new List<IMatchInfoBase>();

        switch (categoryType)
        {
            case CategoryType.Friends:
            {
                List<UserEntity> users;

                switch (type)
                {
                    case GetMatchType.New:
                    {
                        users = await _dataContext.Users
                            .Include(u => u.UserInfo)
                            .ThenInclude(u => u.Images)
                            .Include(u => u.FriendsCategoryInfo)
                            .Include(u => u.ReceivedMatches)
                            .Include(u => u.InitiatedMatches)
                            .Where(u =>
                                u.Id != userId &&
                                u.IsStripeSubscriptionPaid &&
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

                        break;
                    }

                    case GetMatchType.Unanswered:
                    {
                        users = await _dataContext.Matches
                            .Include(m => m.Initiator)
                            .ThenInclude(i => i.UserInfo)
                            .ThenInclude(u => u.Images)
                            .Include(m => m.Initiator)
                            .ThenInclude(i => i.FriendsCategoryInfo)
                            .Where(m => m.ReceiverId == userId && m.IsMatch == false)
                            .Select(m => m.Initiator)
                            .ToListAsync();

                        break;
                    }

                    default:
                    {
                        var matches = await _dataContext.Matches
                            .Where(m => (m.ReceiverId == userId || m.InitiatorId == userId) && m.IsMatch == true)
                            .ToListAsync();

                        var initiatorIds = matches.Select(m => m.InitiatorId);
                        var receiverIds = matches.Select(m => m.ReceiverId);

                        users = await _dataContext.Users
                            .Include(u => u.UserInfo)
                            .ThenInclude(u => u.Images)
                            .Include(u => u.FriendsCategoryInfo)
                            .Where(u => (initiatorIds.Contains(u.Id) || receiverIds.Contains(u.Id)) && u.Id != userId)
                            .ToListAsync();

                        break;
                    }
                }

                infos = users.Select(u => new FriendsMatchInfo
                    {
                        UserId = u.Id,
                        Age = u.UserInfo!.Age,
                        Name = u.UserInfo!.FullName,
                        AboutMe = u.FriendsCategoryInfo!.Info,
                        ImageUrls = u.UserInfo.Images.Select(i => i.Url).ToList(),
                    })
                    .Cast<IMatchInfoBase>()
                    .ToList();

                break;
            }
            case CategoryType.Love:
            {
                List<UserEntity> users;

                switch (type)
                {
                    case GetMatchType.New:
                    {
                        users = await _dataContext.Users
                            .Include(u => u.UserInfo)
                            .ThenInclude(u => u.Images)
                            .Include(u => u.LoveCategoryInfo)
                            .Include(u => u.ReceivedMatches)
                            .Include(u => u.InitiatedMatches)
                            .Where(u =>
                                u.Id != userId &&
                                u.IsStripeSubscriptionPaid &&
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

                        break;
                    }

                    case GetMatchType.Unanswered:
                    {
                        users = await _dataContext.Matches
                            .Include(m => m.Initiator)
                            .ThenInclude(i => i.UserInfo)
                            .ThenInclude(u => u.Images)
                            .Include(m => m.Initiator)
                            .ThenInclude(i => i.LoveCategoryInfo)
                            .Where(m => m.ReceiverId == userId && m.IsMatch == false)
                            .Select(m => m.Initiator)
                            .ToListAsync();

                        break;
                    }

                    default:
                    {
                        var matches = await _dataContext.Matches
                            .Where(m => (m.ReceiverId == userId || m.InitiatorId == userId) && m.IsMatch == true)
                            .ToListAsync();

                        var initiatorIds = matches.Select(m => m.InitiatorId);
                        var receiverIds = matches.Select(m => m.ReceiverId);

                        users = await _dataContext.Users
                            .Include(u => u.UserInfo)
                            .ThenInclude(u => u.Images)
                            .Include(u => u.LoveCategoryInfo)
                            .Where(u => (initiatorIds.Contains(u.Id) || receiverIds.Contains(u.Id)) && u.Id != userId)
                            .ToListAsync();

                        break;
                    }
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
                        ImageUrls = u.UserInfo.Images.Select(i => i.Url).ToList()
                    })
                    .Cast<IMatchInfoBase>()
                    .ToList();

                break;
            }
            case CategoryType.Work:
            {
                List<UserEntity> users;

                switch (type)
                {
                    case GetMatchType.New:
                    {
                        users = await _dataContext.Users
                            .Include(u => u.UserInfo)
                            .ThenInclude(u => u.Images)
                            .Include(u => u.WorkCategoryInfo)
                            .Include(u => u.ReceivedMatches)
                            .Include(u => u.InitiatedMatches)
                            .Where(u =>
                                u.Id != userId &&
                                u.IsStripeSubscriptionPaid &&
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

                        break;
                    }

                    case GetMatchType.Unanswered:
                    {
                        users = await _dataContext.Matches
                            .Include(m => m.Initiator)
                            .ThenInclude(i => i.UserInfo)
                            .ThenInclude(u => u.Images)
                            .Include(m => m.Initiator)
                            .ThenInclude(i => i.WorkCategoryInfo)
                            .Where(m => m.ReceiverId == userId && m.IsMatch == false)
                            .Select(m => m.Initiator)
                            .ToListAsync();

                        break;
                    }

                    default:
                    {
                        var matches = await _dataContext.Matches
                            .Where(m => (m.ReceiverId == userId || m.InitiatorId == userId) && m.IsMatch == true)
                            .ToListAsync();

                        var initiatorIds = matches.Select(m => m.InitiatorId);
                        var receiverIds = matches.Select(m => m.ReceiverId);

                        users = await _dataContext.Users
                            .Include(u => u.UserInfo)
                            .ThenInclude(u => u.Images)
                            .Include(u => u.WorkCategoryInfo)
                            .Where(u => (initiatorIds.Contains(u.Id) || receiverIds.Contains(u.Id)) && u.Id != userId)
                            .ToListAsync();

                        break;
                    }
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
                        ImageUrls = u.UserInfo.Images.Select(i => i.Url).ToList()
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