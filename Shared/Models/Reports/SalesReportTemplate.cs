using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyLook.Shared.Models;
using Shared.Enums;

namespace Shared.Models.Reports;

public class SalesReportTemplate
{
    public string? StoreName { get; set; }
    public string? BranchAddress { get; set; }
    public string? Criteria {  get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<SaleItem> SaleItems { get; set; } = [];
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumBuyPrice => SaleItems.Sum(x => x.BuyPrice);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumSellPrice => SaleItems.Sum(x => x.SellPrice);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumTotalAmount => SaleItems.Sum(x => x.TotalAmount);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumAmountPaid => SaleItems.Sum(x => x.AmountPaid);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumDiscount => SaleItems.Sum(x => x.Discount);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumSubTotal => SaleItems.Sum(x => x.SubTotal);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumBalance => SaleItems.Sum(x => x.Balance);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumRefund => SaleItems.Sum(x => x.Refund);
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumProfit => SaleItems.Sum(x => x.Profit);
}
