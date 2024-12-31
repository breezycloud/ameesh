using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Shared.Models.Products;

[Keyless]
public class Stock
{
    public Guid id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; } = DateTime.UtcNow;
    [Required(ErrorMessage = "Quantity is required")]    
    [Column(TypeName = "decimal(18, 1)")]
    public decimal? Quantity { get; set; }
    [Required(ErrorMessage = "Buy Price is required")]    
    public decimal? BuyPrice { get; set; }    
    public DateTime? ExpiryDate { get; set; }
}

