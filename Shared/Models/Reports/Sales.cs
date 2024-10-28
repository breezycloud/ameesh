using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace Shared.Models.Reports;

public class Sales
{
    public string OrderDate { get; set; }
    public string ReceiptNo { get; set; }
    public string Customer { get; set; }
    public decimal Total { get; set; }        
    public decimal Discount { get; set; }
    public decimal Paid { get; set; }
    public decimal SubTotal => Total - Discount;
    public decimal Balance => SubTotal - Paid;
    public string StartDate { get; set; }
    public string EndDate { get; set; }
}
