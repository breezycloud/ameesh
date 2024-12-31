
using Shared.Enums;
using Shared.Models.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Orders;

public class ProductOrderItem
{
    [Required] public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid StockId { get; set; }
    public string? Product { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal Quantity { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }
    [ForeignKey(nameof(ProductId))]
    public virtual Product? ProductData { get; set; }
}
