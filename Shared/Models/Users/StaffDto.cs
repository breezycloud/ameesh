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
public class StaffDto
{
    public Guid Id { get; set; }
    public Guid? StoreId { get; set; } 
    public string? FirstName { get; set; }    
    public string? LastName { get; set; }    
    public UserRole? Role { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }    
    public override string ToString() => $"{FirstName} {LastName}";
}
