namespace TetaBackend.Features.User.Dto;

public class LoginDto
{
    public string? Email { get; set; }
    
    public string? Phone { get; set; }

    public string Password { get; set; }
}