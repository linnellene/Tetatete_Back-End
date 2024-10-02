namespace TetaBackend.Features.Chat.Dto;

public class MessageDto
{
    public string Content { get; set; }
    
    public Guid SenderId { get; set; }
    
    public Guid ChatId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}