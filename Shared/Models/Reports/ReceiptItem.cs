namespace Shared.Models.Reports;


public class ReceiptItem
{
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string? Cashier { get; set; }
    public string? Item { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal VAT { get; set; }
    public int Quantity { get; set; }    
}

