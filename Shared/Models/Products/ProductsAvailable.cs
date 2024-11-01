using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Products;

public class ProductsAvailable
{
    public Guid Id { get; set; }
    public string? Brand { get; set; }
    public string? Barcode { get; set; }
    public string? ProductName { get; set; }
    public decimal? SellPrice { get; set; }
    [Column(TypeName = "jsonb")]
    public virtual List<Stock> Dispensary { get; set; } = new();
}

