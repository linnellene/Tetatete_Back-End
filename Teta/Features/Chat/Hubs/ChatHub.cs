using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.Features.Chat.Dto;

namespace TetaBackend.Features.Chat.Hubs;

public class ChatHub : Hub
{
    private readonly DataContext _dataContext;

    public ChatHub(DataContext dataContext)
    {
        _dataContext = dataContext;
    }
    
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Connected");
        
        await base.OnConnectedAsync();
    }

    public async Task SendPrivateMessage(Guid userId, string message)
    {
        var senderId = Context.UserIdentifier;

        if (senderId is null)
        {
            throw new ArgumentException("Invalid sender id.");
        }

        var senderIdGuid = new Guid(senderId);

        var room = await _dataContext.Chats.FirstOrDefaultAsync(c =>
            (c.UserAId == senderIdGuid && c.UserBId == userId) || (c.UserBId == senderIdGuid && c.UserAId == userId));

        if (room is null)
        {
            throw new ArgumentException("No rooms with this user.");
        }

        if (room.UserALeft || room.UserBLeft)
        {
            throw new ArgumentException("Cannot send message, one of the users left.");
        }
        
        var newMessage = new MessageEntity
        {
            SenderId = senderIdGuid,
            ChatId = room.Id,
            Content = message,
        };

        _dataContext.Messages.Add(newMessage);
        
        await _dataContext.SaveChangesAsync();

        var messageToSend = new SendMessageDto
        {
            SenderId = senderId,
            ChatId = room.Id.ToString(),
            Content = message,
        };

        await Clients.User(userId.ToString()).SendAsync("ReceiveMessage", messageToSend);
        await Clients.User(senderId).SendAsync("ReceiveMessage", messageToSend);
    }
}