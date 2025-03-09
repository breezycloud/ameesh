using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyLook.Shared.Models;
using Shared.Enums;
using Shared.Models.Products;

namespace Shared.Models.Reports;

public class ThirdpartySalesReportTemplate
{
    public string? StoreName { get; set; }
    public string? BranchAddress { get; set; }
    public string? Criteria {  get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<ThirdpartySalesReport> SalesReport { get; set; } = [];
    // public string? ReceiptNo { get; set; }
    // public string? Customer { get; set; }
    // public List<ThirdPartyItem> SaleItems { get; set; } = [];
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumBuyPrice => SalesReport.Sum(x => x.SaleItems.Sum(x => x.Cost));
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumSellPrice => SalesReport.Sum(x => x.SaleItems.Sum(x => x.Price));
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumTotalAmount => SalesReport.Sum(x => x.SaleItems.Sum(x => x.Total));
    public decimal SumProfit => SumSellPrice - SumBuyPrice;
}

public class ThirdpartySalesReport
{    
    public string? ReceiptNo { get; set; }
    public string? Customer { get; set; }
    public List<ThirdPartyItem> SaleItems { get; set; } = [];
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumBuyPrice => SaleItems.Sum(x => x.Cost);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumSellPrice => SaleItems.Sum(x => x.Price);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumTotalAmount => SaleItems.Sum(x => x.Total);
    public decimal SumProfit => SumSellPrice - SumBuyPrice;
}
