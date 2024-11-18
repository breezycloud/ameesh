using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.Models.Products;

public class ProductByStore
{
    public Guid Id { get; set; }
    public string? BrandName { get; set; }  
    public string? CategoryName { get; set; }  
    public string? ProductName { get; set; }
    public string? StoreName { get; set; }
    public decimal CostPrice { get; set; }
     public MarkupType MarkupType { get; set; } = MarkupType.Fixed;
    public decimal MarkupAmount { get; set; }
    public decimal MarkupPercentage { get; set; }
    public decimal Price { get; set; }
    public decimal DispensaryQuantity {  get; set; }
    public decimal StoreQuantity {  get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public List<Stock> Stocks { get; set; } = new();
    public List<Stock> Dispensary { get; set; } = new();
    public override string ToString() => $"{BrandName} {ProductName}";
}

