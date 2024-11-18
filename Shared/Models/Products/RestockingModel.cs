using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Products;

public class RestockingModel
{    
    public string? Option { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }    
    public decimal CurrentQty { get; set; }
    public decimal? NewQty { get; set; }
    public Guid StoreID { get; set; }
    public Guid StockID { get; set; }
    public DateTime? ExpiryDate { get; set; }

}
