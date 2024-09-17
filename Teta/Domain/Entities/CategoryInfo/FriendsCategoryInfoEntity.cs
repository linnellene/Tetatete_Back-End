using System.ComponentModel.DataAnnotations;
using TetaBackend.Domain.Entities.Base;
using TetaBackend.Features.Interfaces;

namespace TetaBackend.Domain.Entities.CategoryInfo;

public class FriendsCategoryInfoEntity : BaseEntity, ICategory
{
    [MinLength(10)] [MaxLength(1000)] public string Info { get; set; }

    public Guid UserId { get; set; }

    public UserEntity User { get; set; }
}