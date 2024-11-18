using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models.Orders;
using Shared.Models.Company;
using Shared.Enums;

namespace Shared.Models.Products;

public class Product
{
    [Key]
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public Guid StoreId { get; set; }    
    public MarkupType MarkupType { get; set; } = MarkupType.Fixed;
    public decimal MarkupAmount { get; set; }
    public decimal MarkupPercentage { get; set; }

    [Required(ErrorMessage = "Sell Price is required")]
    public decimal? SellPrice { get; set; }

    public decimal DispensaryQuantity => Dispensary.Where(x => x.Quantity > 0).Sum(x => x.Quantity).GetValueOrDefault();
    public decimal QuantitySold => OrderItems.Where(x => x.Status == OrderStatus.Completed).Sum(x => x.Quantity);    
    public decimal QuantityPending => OrderItems.Where(x => x.Status == OrderStatus.Pending).Sum(x => x.Quantity);    
    public decimal QuantityCancelled => OrderItems.Where(x => x.Status == OrderStatus.Canceled).Sum(x => x.Quantity);    
    public decimal StoreQuantity => Stocks.Where(x => x.Quantity > 0).Sum(x => x.Quantity).GetValueOrDefault();
    public int ReorderLevel { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; }
    [Column(TypeName = "jsonb")]
    public virtual List<Stock> Dispensary { get; set; } = new();
    [Column(TypeName = "jsonb")]
    public virtual List<Stock> Stocks { get; set; } = new();
    public virtual List<ReturnedProduct> ReturnedProducts { get; set; } = new();
    [ForeignKey(nameof(StoreId))]
    public virtual Store? Store { get; set; }
    [ForeignKey(nameof(ItemId))]
    public virtual Item? Item { get; set; }
    public virtual ICollection<ProductOrderItem> OrderItems { get; set; } = new List<ProductOrderItem>();
}
