using System.ComponentModel.DataAnnotations.Schema;
using TetaBackend.Domain.Entities.Base;
using TetaBackend.Domain.Enums;

namespace TetaBackend.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; }

    public string Password { get; set; }
    
    [Column(TypeName = "smallint")]
    public int Age { get; set; }
    
    public string About { get; set; }
    
    public Gender Gender { get; set; }
    
    public string PlaceOfBirth { get; set; }
    
    public string Location { get; set; }
    
    public string Languages { get; set; }
    
    public string FullName { get; set; }
}