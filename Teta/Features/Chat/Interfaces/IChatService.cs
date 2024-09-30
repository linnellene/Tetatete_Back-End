using TetaBackend.Domain.Entities;
using TetaBackend.Features.Chat.Dto;

namespace TetaBackend.Features.Chat.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatDto>> GetUserChatsWithLastMessages(Guid userId);

    Task<IEnumerable<MessageEntity>> GetChatMessages(Guid userId, Guid chatId);

    Task JoinChat(Guid userId, Guid chatId);
    
    Task LeaveChat(Guid userId, Guid chatId);
}