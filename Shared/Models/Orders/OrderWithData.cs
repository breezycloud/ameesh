using Shared.Enums;
using Shared.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Orders;

public class OrderWithData
{
    public Guid Id { get; set; }
    public string? StoreName { get; set; }
    public string? OrderType { get; set; }
    public string? ReceiptNo { get; set; }
    public DateOnly Date { get; set; }        
    public string? CustomerName { get; set; }
    public bool IsHasDiscount { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Consultation { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Profit { get; set;}
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Discount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SubTotal { get; set; }
    public decimal Balance { get; set; }
    public string? PaymentStatus { get; set; }
    public bool HasDelievery { get; set; }
    public bool Dispatched { get; set; }
    public string? DeliveryStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool DiscountAboveLimit()
    {        
        var EligibleDiscount =  TotalAmount * (IsHasDiscount ? 0.1M :  0.05M);
        if (Discount > EligibleDiscount)
            return true;
        else
            return false;
    }
    public bool HasReturns { get; set; } = false;
    public bool HasOtherServices { get; set; } = false;
    public bool HasOrderItems { get; set; } = false;
    public string? Sale { get; set;}
    public string? Cashier { get; set;}
    public string? ReferredBy { get; set;}
}
