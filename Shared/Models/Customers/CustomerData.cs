namespace Shared.Models.Customers;

#nullable disable
public class CustomerData
{    
    public Guid Id { get; set; }
    public string CustomerName { get; set; }
    public string PhoneNo { get; set; }
    public string ContactAddress { get; set; }
    public string PhoneNo2 { get; set; }
    public string ContactAddress2 { get; set; }   
    public int TotalSales { get; set; }
    public bool IsWalkIn { get; set; }
    public DateTime CreatedDate { get; set; }
    //public virtual List<ReceiptItem> OrderDetails { get; set; } = new();
    //public virtual List<ProductOrderDetails> ProductOrderDetails { get; set; } = new();
}