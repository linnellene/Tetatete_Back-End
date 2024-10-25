namespace TetaBackend.Features.Chat.Dto;

public class SendMessageDto
{
    public string MessageId { get; set; }
    
    public string Content { get; set; }
    
    public bool SentByUser { get; set; }
    
    public string ChatId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}