using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class UserInfoLanguageEntity : BaseEntity
{
    public Guid UserInfoId { get; set; }

    public Guid LanguageId { get; set; }

    public UserInfoEntity UserInfo { get; set; }

    public LanguageEntity Language { get; set; }
}