namespace TetaBackend.Features.User.Interfaces;

public interface IEmailService
{
    Task SendEmail(string toEmail, string text);
}