using TetaBackend.Domain.Entities;
using TetaBackend.Features.User.Dto;

namespace TetaBackend.Features.User.Interfaces;

public interface IUserService
{
    Task<bool> CreateUser(string username, string password);
    
    Task ValidateRegisterParameters(string username, string password, bool logIn = false);

    Task<UserEntity?> Authenticate(string username, string password);

    Task<UserInfoEntity?> GetUserInfo(Guid userId);

    Task<UserInfoEntity> FillInformation(Guid userId, FillUserInfoDto dto);
    
    Task<UserInfoEntity> UpdateInformation(Guid userId, UpdateUserInfoDto dto);
}