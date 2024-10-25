namespace TetaBackend.Features.Chat.Dto;

public class ChatDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public Guid CompanionId { get; set; }
    
    public string CompanionName { get; set; }
    
    public string CompanionPictureUrl { get; set; }
    
    public bool CompanionLeft { get; set; }
    
    public bool UserLeft { get; set; }
    
    public DateTimeOffset MatchedSince { get; set; }
    
    public MessageDto? LastMessage { get; set; }
}