using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Models.Products;


namespace Shared.Models.Reports;


public class UserSoldProduct
{
    public DateTime ReportDate { get; set; }
    public string? User { get; set; }    
    public List<SoldProducts> Products { get; set; } = [];
    public int SumTotalQty => Products.Sum(s => s.QtySold);
    public int SumTotalDispensary => Products.Sum(s => s.DispensaryQty);
}