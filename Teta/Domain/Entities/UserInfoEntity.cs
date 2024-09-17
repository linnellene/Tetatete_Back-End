using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class UserInfoEntity : BaseEntity
{
    [Column(TypeName = "smallint")] public int Age { get; set; }

    [MinLength(10)] public string About { get; set; }

    [MinLength(3)] public string FullName { get; set; }

    public string ImageUrl { get; set; }

    public Guid UserId { get; set; }

    public Guid GenderId { get; set; }

    public Guid PlaceOfBirthId { get; set; }

    public Guid LocationId { get; set; }

    public UserEntity User { get; set; }

    public GenderEntity Gender { get; set; }

    [ForeignKey("PlaceOfBirthId")] public LocationEntity PlaceOfBirth { get; set; }

    [ForeignKey("LocationId")] public LocationEntity Location { get; set; }

    public ICollection<UserInfoLanguageEntity> UserInfoLanguages { get; set; }
}