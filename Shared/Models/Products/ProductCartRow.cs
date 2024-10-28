using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Products;
public class ProductCartRow
{
    public decimal GrandTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; } = 0;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost {  get; set; } = 0;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total => Quantity * Cost;
    public Product? Product { get; set; } = new();
    public string? SearchByBarcode { get; set; }
}
