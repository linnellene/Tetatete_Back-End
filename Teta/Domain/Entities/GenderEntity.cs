using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class GenderEntity: BaseEntity
{
    public string Name { get; set; }
    
    public ICollection<UserInfoEntity> UserInfoEntities { get; set; }
}