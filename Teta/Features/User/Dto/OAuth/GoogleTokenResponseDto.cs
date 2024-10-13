namespace TetaBackend.Features.User.Dto.OAuth;

public class GoogleTokenResponseDto
{
    public string access_token { get; set; }
    public string id_token { get; set; }
}
