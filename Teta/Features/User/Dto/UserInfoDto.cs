namespace TetaBackend.Features.User.Dto;

public class UserInfoDto
{
    public int Age { get; set; }
    
    public string About { get; set; }
    
    public string FullName { get; set; }
    
    public List<string> ImageUrls { get; set; }
    
    public Guid GenderId { get; set; }
    
    public Guid PlaceOfBirthId { get; set; }
    
    public Guid LocationId { get; set; }
    
    public ICollection<Guid> LanguageIds { get; set; }
}