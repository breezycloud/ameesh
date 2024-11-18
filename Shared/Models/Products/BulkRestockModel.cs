using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.Models.Products;

public class BulkRestockModel
{
    public IEnumerable<Product>? Products { get; set;} = new HashSet<Product>();
    public decimal? NewQuantity { get; set;}
}

public class BulkRestockDispensary
{
    [Key]
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid StoreId { get; set; }
    public Guid StockId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string? ProductName { get; set; }
    public decimal Price { get; set; }
    public decimal _Dispensary { get; set; }
    public decimal CurrentDispensary { get; set; }
    public decimal QuantitySold { get; set; }
    public decimal StoreQuantity { get; set; }
    public decimal CurrentStoreQuantity { get; set; }
    public DateTime? ExpiryDate { get; set; }
    //public decimal QtyOnHand => Dispensary.Sum(x => x.Quantity!.Value) - OrderItems.Where(x => x.Status != OrderStatus.Canceled).Sum(x => x.Quantity);
    public decimal? NewQuantity { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; }    
}


