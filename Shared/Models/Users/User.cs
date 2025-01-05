using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;
using Shared.Models.Company;

using Shared.Models.Welfare;


namespace Shared.Models.Users;
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? StoreId { get; set; }
    [Required(ErrorMessage = "First Name is required")]
    public string? FirstName { get; set; }
    [Required(ErrorMessage = "Last Name is required")]
    public string? LastName { get; set; }    
    public UserRole Role { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    [ForeignKey(nameof(StoreId))]
    public virtual Store? Store { get; set; }    
    public virtual UserCredential? UserCredential { get; set; } = null;
    public virtual List<Salary> Salaries { get; set; } = [];
    public virtual List<SalaryAdvance> SalaryAdvances { get; set; } = [];
    public virtual List<SalaryBonus> SalaryBonus { get; set; } = [];
    public virtual List<Penalty> Penalties { get; set; } = [];
    public override string ToString() => $"{FirstName} {LastName}";
}
