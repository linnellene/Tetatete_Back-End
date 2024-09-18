using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class ImageEntity : BaseEntity
{
    public string Url { get; set; }
    
    public Guid UserInfoId { get; set; }
    
    public UserInfoEntity UserInfo { get; set; }
}