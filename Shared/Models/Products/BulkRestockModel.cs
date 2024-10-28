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
    public int? NewQuantity { get; set;}
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
    public int _Dispensary { get; set; }
    public int CurrentDispensary { get; set; }
    public int QuantitySold { get; set; }
    public int StoreQuantity { get; set; }
    public int CurrentStoreQuantity { get; set; }
    public DateTime? ExpiryDate { get; set; }
    //public int QtyOnHand => Dispensary.Sum(x => x.Quantity!.Value) - OrderItems.Where(x => x.Status != OrderStatus.Canceled).Sum(x => x.Quantity);
    public int? NewQuantity { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; }    
}


