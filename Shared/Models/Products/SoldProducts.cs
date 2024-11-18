using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Products;

public class SoldProducts
{
    public Guid Id { get; set; }
    public string? ProductName { get; set; }
    public decimal QtySold { get; set; }
    public decimal DispensaryQty { get; set; }
}