using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace Shared.Models.Reports;
public class Receipt
{
    public Guid OrderID { get; set; }
    public string CustomerName { get; set; }
    public string OrderDate { get; set; }
    public DateTime? LastPayDate { get; set; }
    public DateTime? NewPayDate { get; set; }
    public string ReceiptNo { get; set; }
    public dynamic Quantity { get; set; }
    public string CatalogName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Cost { get; set; }
    public decimal SubTotal => TotalAmount - Discount;
    public decimal PreviousPayment { get; set; }
    public decimal PreviousPayments { get; set; }
    public decimal Balance => SubTotal - PreviousPayments - AmountPaid;
    public string Cashier { get; set; }
    public byte[] QrCode { get; set; }
}
