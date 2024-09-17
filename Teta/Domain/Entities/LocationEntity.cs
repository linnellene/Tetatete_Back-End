using TetaBackend.Domain.Entities.Base;

namespace TetaBackend.Domain.Entities;

public class LocationEntity : BaseEntity
{
    public string City { get; set; }

    public string Country { get; set; }

    public ICollection<UserInfoEntity> UserInfoBirthPlaces { get; set; }

    public ICollection<UserInfoEntity> UserInfoLocations { get; set; }
}