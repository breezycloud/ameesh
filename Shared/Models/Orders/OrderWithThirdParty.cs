using Shared.Enums;
using Shared.Models.Products;
using Shared.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Orders;

public class OrderWithThirdParty
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }    
    public string? Customer { get; set; }
    public bool IsHasDiscount { get; set; }
    public string? ReceiptNo { get; set; }
    public string? Option { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal OtherService { get; set; }
    [Column(TypeName = "decimal(18,1)")]
    public decimal Quantity { get; set; } = 0;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal BuyPrice { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SellPrice { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal AmountPaid { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount {get; set;}
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Discount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SubTotal {get; set;}
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Balance { get; set; }    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Profit { get; set; }
    public decimal ReturnsQty { get; set; }
    public decimal Refund { get; set;}
    public bool HasReturns { get; set; } = false;
    public bool HasOtherServices { get; set; } = false;
    public bool HasOrderItems { get; set; } = false;
    public bool HasThirdItems { get; set; } = false;
    public IReadOnlyCollection<OrderItemDetail> OrderItems { get; set; } = [];
    public IReadOnlyCollection<ThirdPartyItem> ThirdPartyItems { get; set; } = [];
    public string? Sale { get; set;}
    public string? Cashier { get; set;}
    public string? ReferredBy { get; set;}
    public bool DiscountAboveLimit()
    {        
        var EligibleDiscount =  TotalAmount * (IsHasDiscount ? 0.1M :  0.05M);
        if (Discount > EligibleDiscount)
            return true;
        else
            return false;
    }
     public string Remark  => GetRemark() ?? string.Empty;
    public string GetRemark() 
    {
        string remark = "";
        if (IsHasDiscount && Balance > 0)
            remark = "F & F";
        else if (DiscountAboveLimit())
            remark = "Exchange";
        else if (HasReturns)
            remark = "Returns";

        return remark;
    }

}
