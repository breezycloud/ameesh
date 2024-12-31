using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Users;


public class UserCredential
{
    [Key]    
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }        
    [DataType(DataType.Password)]
    public string? HashedPassword { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsNew { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}