using System.ComponentModel.DataAnnotations;
using TetaBackend.Domain.Entities.Base;
using TetaBackend.Features.Interfaces;

namespace TetaBackend.Domain.Entities.CategoryInfo;

public class LoveCategoryInfoEntity : BaseEntity, ICategory
{
    [MinLength(10)] [MaxLength(1000)] public string Info { get; set; }

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public Guid GenderId { get; set; }

    public Guid UserId { get; set; }

    public GenderEntity Gender { get; set; }

    public UserEntity User { get; set; }
}