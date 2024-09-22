namespace TetaBackend.Features.User.Dto;

public class FillUserInfoDto
{
    public int Age { get; set; }
    
    public string About { get; set; }
    
    public string FullName { get; set; }
    
    public List<IFormFile> Images { get; set; }
    
    public Guid GenderId { get; set;}
    
    public Guid PlaceOfBirthId { get; set;}
    
    public Guid LocationId { get; set; }
    
    public IEnumerable<Guid> Languages { get; set; }
}