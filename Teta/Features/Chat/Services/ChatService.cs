using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.Features.Chat.Dto;
using TetaBackend.Features.Chat.Interfaces;

namespace TetaBackend.Features.Chat.Services;

public class ChatService : IChatService
{
    private readonly DataContext _dataContext;

    public ChatService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IEnumerable<ChatDto>> GetUserChatsWithLastMessages(Guid userId)
    {
        if (!await _dataContext.Users.AnyAsync(u => u.Id == userId))
        {
            throw new ArgumentException("Invalid user id.");
        }

        var chats = await _dataContext.Chats
            .Include(c => c.Messages)
            .Include(c => c.UserA)
            .ThenInclude(u => u.UserInfo)
            .ThenInclude(ui => ui.Images)
            .Include(c => c.UserB)
            .ThenInclude(u => u.UserInfo)
            .ThenInclude(ui => ui.Images)
            .Where(c => c.UserAId == userId || c.UserBId == userId)
            .Select(c =>
                new ChatDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CompanionId = c.UserAId == userId ? c.UserBId : c.UserAId,
                    CompanionLeft = c.UserAId == userId ? c.UserBLeft : c.UserALeft,
                    CompanionName = c.UserAId == userId ? c.UserB.UserInfo!.FullName : c.UserA.UserInfo!.FullName,
                    CompanionPictureUrl =
                        c.UserAId == userId
                            ? c.UserB.UserInfo!.Images.FirstOrDefault()!.Url
                            : c.UserA.UserInfo!.Images.FirstOrDefault()!.Url,
                    UserLeft = c.UserAId == userId ? c.UserALeft : c.UserBLeft,
                    MatchedSince = c.CreatedAt,
                    LastMessage = c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => new MessageDto
                    {
                        MessageId = m.Id.ToString(),
                        ChatId = m.ChatId,
                        Content = m.Content,
                        SentByUser = m.SenderId == userId,
                        Timestamp = m.CreatedAt,
                    }).FirstOrDefault(),
                }
            )
            .ToListAsync();

        return chats;
    }

    public async Task<IEnumerable<MessageEntity>> GetChatMessages(Guid userId, Guid chatId)
    {
        var chat = await _dataContext.Chats.FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat is null)
        {
            throw new ArgumentException("Invalid chat id.");
        }

        if (!(chat.UserAId == userId || chat.UserBId == userId))
        {
            throw new ArgumentException("User is not in chat.");
        }

        var messages = await _dataContext.Messages.Where(m => m.ChatId == chatId).ToListAsync();

        return messages;
    }

    public async Task JoinChat(Guid userId, Guid chatId)
    {
        if (!await _dataContext.Users.AnyAsync(u => u.Id == userId))
        {
            throw new ArgumentException("Invalid user id.");
        }

        var chat = await _dataContext.Chats.FirstOrDefaultAsync(c => c.Id == chatId);

        var isUserAChat = chat?.UserAId == userId;
        var isUserBChat = chat?.UserBId == userId;

        if (chat is null || !(isUserAChat || isUserBChat))
        {
            throw new ArgumentException("Invalid chat id.");
        }

        if ((isUserAChat && !chat.UserALeft) || (isUserBChat && !chat.UserBLeft))
        {
            throw new ArgumentException("Already joined.");
        }

        if (isUserAChat)
        {
            chat.UserALeft = false;
        }
        else
        {
            chat.UserBLeft = false;
        }

        await _dataContext.SaveChangesAsync();
    }

    public async Task LeaveChat(Guid userId, Guid chatId)
    {
        if (!await _dataContext.Users.AnyAsync(u => u.Id == userId))
        {
            throw new ArgumentException("Invalid user id.");
        }

        var chat = await _dataContext.Chats.FirstOrDefaultAsync(c => c.Id == chatId);

        var isUserAChat = chat?.UserAId == userId;
        var isUserBChat = chat?.UserBId == userId;

        if (chat is null || !(isUserAChat || isUserBChat))
        {
            throw new ArgumentException("Invalid chat id.");
        }

        if ((isUserAChat && chat.UserALeft) || (isUserBChat && chat.UserBLeft))
        {
            throw new ArgumentException("Already left.");
        }

        if (isUserAChat)
        {
            chat.UserALeft = true;
        }
        else
        {
            chat.UserBLeft = true;
        }

        await _dataContext.SaveChangesAsync();
    }
}