namespace TetaBackend.Features.Chat.Dto;

public class ChatDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public Guid UserAId { get; set; }
    
    public Guid UserBId { get; set; }
    
    public bool UserALeft { get; set; }
    
    public bool UserBLeft { get; set; }
    
    public MessageDto? LastMessage { get; set; }
}