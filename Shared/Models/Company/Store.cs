using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.Orders;

namespace Shared.Models.Company;

public class Store
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "Name is required")]
    public string? BranchName { get; set; } = "Ameesh Luxury";
    public string? BranchAddress { get; set; }
    [Required(ErrorMessage = "Phone No is required")]
    public string? PhoneNo1 { get; set; }
    public string? PhoneNo2 { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = [];

}
