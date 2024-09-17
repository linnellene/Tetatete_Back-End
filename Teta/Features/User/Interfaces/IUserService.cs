using TetaBackend.Domain.Entities;
using TetaBackend.Features.Interfaces;
using TetaBackend.Features.User.Dto;
using TetaBackend.Features.User.Enums;

namespace TetaBackend.Features.User.Interfaces;

public interface IUserService
{
    Task<bool> CreateUser(string username, string password);

    Task ValidateRegisterParameters(string username, string password, bool logIn = false);

    Task<UserEntity?> Authenticate(string username, string password);

    Task<UserInfoEntity?> GetUserInfo(Guid userId);

    Task<UserInfoEntity> FillInformation(Guid userId, FillUserInfoDto dto);

    Task<UserInfoEntity> UpdateInformation(Guid userId, UpdateUserInfoDto dto);

    Task<CategoryType?> GetFulfilledInfoType(Guid userId);

    Task<TCategory?> GetCategoryInfo<TCategory>(Guid userId) where TCategory : class, ICategory;

    Task FillCategoryInfo<TCategory>(Guid userId, TCategory info)
        where TCategory : class, ICategory;

    Task UpdateCategoryInfo<TCategory>(Guid categoryId, TCategory info)
        where TCategory : class, ICategory;

    Task DeleteCategoryInfo(CategoryType type, Guid categoryId);
}