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
    public decimal TotalQuantity { get;set; }
    public decimal OldQuantity { get;set; }
    [Required(ErrorMessage = "Quantity is required")]
    public decimal? NewQuantity { get;set; }
    public string? Prescription { get; set; }

}
