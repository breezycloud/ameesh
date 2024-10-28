using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Products;

public class ProductReportTemplate
{
    public string? StoreName { get; set; }
    public List<ProductItems> Items { get; set; } = [];
    public decimal SumDispensaryQty => Items.Sum(x => x.DispensaryQuantity);
    public decimal SumStoreQty => Items.Sum(x => x.StoreQuantity);
    public decimal SumCostPrice => Items.Sum(x => x.CostPrice);
    public decimal SumSellPrice => Items.Sum(x => x.SellPrice);
    public decimal SumProjection => Items.Sum(x => x.Projection);
}

public class ProductItems
{
    public string? BrandName { get; set; }
    public string? CategoryName { get; set; }
    public string? ProductName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int DispensaryQuantity { get; set; }
    public int StoreQuantity { get; set; }
    public decimal Projection => (DispensaryQuantity + StoreQuantity) * SellPrice;
}
