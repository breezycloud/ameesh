using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Orders;

public class CartRowUpdate
{
    [Key]
    public Guid id { get; set; }
    public int TotalQuantity { get;set; }
    public int OldQuantity { get;set; }
    [Required(ErrorMessage = "Quantity is required")]
    public int? NewQuantity { get;set; }
    public string? Prescription { get; set; }

}
