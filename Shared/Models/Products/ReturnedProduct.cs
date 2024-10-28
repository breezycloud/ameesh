using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Models.Orders;

namespace Shared.Models.Products;

public class ReturnedProduct
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    [Required(ErrorMessage = "Quantity is required")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cost { get; set; }
    public string? RefundType { get; set;} = "Cash";
    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

}