namespace TetaBackend.Features.Chat.Dto;

public class SendMessageDto
{
    public string Content { get; set; }
    
    public string SenderId { get; set; }
    
    public string ChatId { get; set; }
}