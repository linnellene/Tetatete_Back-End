using TetaBackend.Domain.Entities;

namespace TetaBackend.Features.User.Interfaces;

public interface IUserService
{
    Task<bool> CreateUser(string username, string password);
    
    Task ValidateRegisterParameters(string username, string password, bool logIn = false);

    Task<UserEntity?> Authenticate(string username, string password);
}