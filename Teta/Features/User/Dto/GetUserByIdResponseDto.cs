namespace TetaBackend.Features.User.Dto;

public class GetUserByIdResponseDto
{
    public string About { get; set; }

    public int Age { get; set; }

    public string FullName { get; set; }

    public string Gender { get; set; }

    public IEnumerable<string> Languages { get; set; }

    public string Location { get; set; }

    public string PlaceOfBirth { get; set; }

    public IEnumerable<string> ProfilePictureUrls { get; set; }
}