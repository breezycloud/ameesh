using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyLook.Shared.Models;
using Shared.Enums;

namespace Shared.Models.Reports;

public class SummaryReportTemplate
{
    public string? StoreName { get; set; }
    public string? BranchAddress { get; set; }
    public string? Criteria {  get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumBuyPrice { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumSellPrice { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumTotalAmount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumAmountPaid { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumDiscount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumSubTotal { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumBalance { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumRefund { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SumProfit { get; set; }
}
