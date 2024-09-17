using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class LanguageEntity : BaseEntity
{
    public string Name { get; set; }

    public ICollection<UserInfoLanguageEntity> UserInfoLanguages { get; set; }
}