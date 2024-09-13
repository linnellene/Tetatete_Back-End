using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class UserEntity : BaseEntity
{
    public string Username { get; set; }

    public string Password { get; set; }
    
    public Guid? UserInfoId { get; set; }

    public UserInfoEntity? UserInfo;
}