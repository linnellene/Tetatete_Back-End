namespace TetaBackend.User.Interfaces;

public interface IUserService
{
    Task<bool> CreateUser(string username, string password);
    
    Task<bool> ValidateRegisterParameters(string username, string password);
}