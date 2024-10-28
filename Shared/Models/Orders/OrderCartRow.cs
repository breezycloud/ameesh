
using Shared.Models.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Orders;

public class OrderCartRow
{
    public decimal GrandTotal { get; set; }   
    public int Quantity { get; set; } = 0;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Cost => Product!.SellPrice!.Value;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal BuyPrice => Stock!.BuyPrice!.Value;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total => Quantity * Cost;
    public ProductsAvailable? Product { get; set; } = new();
    public Stock? Stock { get; set; } = new();
    public string? ItemName => Product!.ProductName;
    public string? SearchByBarcode { get; set; }
    public string? SelectedOption { get; set; }
}
