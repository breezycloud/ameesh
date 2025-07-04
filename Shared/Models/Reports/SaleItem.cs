using Shared.Enums;
using Shared.Models.Customers;
using Shared.Models.Orders;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

namespace HyLook.Shared.Models;
public class SaleItem
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public DateOnly Date { get; set; }    
    public string? Customer { get; set; }
    [NotMapped]
    public bool IsHasDiscount { get; set; }
    public string? ReceiptNo { get; set; }
    [NotMapped]
    public string? Option { get; set; }
    [NotMapped]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal OtherService { get; set; }
    [NotMapped]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; } = 0;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal BuyPrice { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SellPrice { get; set; }
    [NotMapped]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal AmountPaid { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount {get; set;}
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Discount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SubTotal {get; set;}
    [NotMapped]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Balance { get; set; }    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Profit { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TP_Total { get; set; }
    public bool DiscountAboveLimit()
    {
        var EligibleDiscount =  TotalAmount * 0.05M;
        if (Discount > EligibleDiscount)
            return true;
        else
            return false;
    }
    [NotMapped]
    public decimal ReturnsQty { get; set; }
    [NotMapped]
    public decimal Refund { get; set;}    
    public string Remark  => GetRemark() ?? string.Empty;
    public string GetRemark() 
    {
        string remark = "";
        if (IsHasDiscount && Balance > 0)
            remark = "F & F";
        else if (DiscountAboveLimit())
            remark = "Discount";        

        return remark;
    }
}