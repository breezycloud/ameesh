namespace Shared.Models.Reports;

public class ReceiptModel 
{
    public bool IsPrint { get; set; }
    public string? ReceiptNo { get; set; }
    public string? Customer { get; set; }
    public string? PhoneNo { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Date { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Balance { get; set; }
    public decimal OutstandingBalance { get; set; }
    public List<ReceiptItem> ReceipItems { get; set; } = new();

}