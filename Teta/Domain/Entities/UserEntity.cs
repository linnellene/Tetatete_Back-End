using TetaBackend.Domain.Entities.Base;
using TetaBackend.Domain.Entities.CategoryInfo;

namespace TetaBackend.Domain.Entities;

public class UserEntity : BaseEntity
{
    public string Username { get; set; }

    public string Password { get; set; }

    public Guid? UserInfoId { get; set; }

    public Guid? LoveCategoryInfoId { get; set; }

    public Guid? FriendsCategoryInfoId { get; set; }

    public Guid? WorkCategoryInfoId { get; set; }

    public UserInfoEntity? UserInfo { get; set; }

    public LoveCategoryInfoEntity? LoveCategoryInfo { get; set; }

    public FriendsCategoryInfoEntity? FriendsCategoryInfo { get; set; }

    public WorkCategoryInfoEntity? WorkCategoryInfo { get; set; }
}