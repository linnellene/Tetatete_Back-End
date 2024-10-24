using TetaBackend.Domain.Entities;
using TetaBackend.Features.Interfaces;
using TetaBackend.Features.User.Dto;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.User.Interfaces;

public interface IUserService
{
    Task<IEnumerable<GenderEntity>> GetAllGenders();
    
    Task<bool> CheckIfUsersAreInChat(Guid userAId, Guid userBId);
    
    Task<IEnumerable<LocationEntity>> GetAllLocations();
        
    Task<IEnumerable<LanguageEntity>> GetAllLanguages();
    
    Task CreateUser(string email, string phone, string password);

    Task<UserEntity?> Authenticate(string password, string? email, string? phone);

    Task<UserInfoEntity?> GetUserInfo(Guid userId);

    Task<UserInfoEntity> FillInformation(Guid userId, FillUserInfoDto dto);

    Task<UserInfoEntity> UpdateInformation(Guid userId, UpdateUserInfoDto dto);

    Task<CategoryType?> GetFulfilledInfoType(Guid userId);

    Task<TCategory?> GetCategoryInfo<TCategory>(Guid userId) where TCategory : class, ICategory;

    Task FillCategoryInfo<TCategory>(Guid userId, TCategory info)
        where TCategory : class, ICategory;

    Task UpdateCategoryInfo<TCategory>(Guid userId, Guid categoryId, TCategory info)
        where TCategory : class, ICategory;

    Task DeleteCategoryInfo(CategoryType type, Guid userId);

    Task SendForgotPasswordEmail(string email);
    
    Task UpdatePassword(Guid userId, string password);
    
    string GenerateGoogleLoginLink();

    Task<string> AuthorizeUserFromGoogleAsync(string code);
    
    string GenerateFacebookLoginLink();

    Task<string> AuthorizeUserFromFacebookAsync(string code);
}