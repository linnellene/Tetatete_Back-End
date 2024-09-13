using System.ComponentModel.DataAnnotations.Schema;
using TetaBackend.Domain.Entities.Base;
using TetaBackend.Domain.Enums;

namespace TetaBackend.Domain.Entities;

public class UserInfoEntity: BaseEntity
{
    [Column(TypeName = "smallint")]
    public int Age { get; set; }
    
    public string About { get; set; }
    
    public Gender Gender { get; set; }
    
    public string PlaceOfBirth { get; set; }
    
    public string Location { get; set; }
    
    public string Languages { get; set; }
    
    public string FullName { get; set; }
    
    public int UserId { get; set; }

    public UserEntity User;
}