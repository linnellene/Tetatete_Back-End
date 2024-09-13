namespace TetaBackend.Features.User.Dto;

public class UpdateUserInfoDto
{
    public int? Age { get; set; }
    
    public string? About { get; set; }
    
    public string? FullName { get; set; }
    
    public IFormFile? Image { get; set; }
    
    public Guid? GenderId { get; set; }
    
    public Guid? PlaceOfBirthId { get; set; }
    
    public Guid? LocationId { get; set; }
    
    public IEnumerable<Guid>? Languages { get; set; }
}