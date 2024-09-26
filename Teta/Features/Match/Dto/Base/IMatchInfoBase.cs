namespace TetaBackend.Features.Match.Dto.Base;

public interface IMatchInfoBase
{
    public Guid UserId { get; set; }
    
    public string Name { get; set; }
    
    public int Age { get; set; }
}