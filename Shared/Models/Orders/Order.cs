using Shared.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Shared.Enums;
using System.Text.Json.Serialization;
using Shared.Models.Users;
using Shared.Models.Company;
using Shared.Models.Products;
using Shared.Models.Customers;


namespace Shared.Models.Orders;

public class Order
{
    [Key]
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid UserId { get; set; }
    public Guid StoreId { get; set; }
    public string? ReceiptNo { get; set; }
    public DateOnly OrderDate { get; set; }   
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount => ProductOrders.Sum(x => x.Quantity * x.Cost) + ThirdPartyItems.Sum(x => x.Total) + DeliveryAmt;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Discount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal DeliveryAmt { get; set; } = 0M;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal SubTotal => TotalAmount - Discount;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Balance => SubTotal - Payments.Sum(p => p.Amount);
    public bool PaymentConfirmed { get; set; }
    public PaymentStatus GetPaymentStatus()
    {
        if (Balance == 0 && !PaymentConfirmed)
            return PaymentStatus.Awaiting;
        else if (Balance == 0 && PaymentConfirmed)
            return PaymentStatus.Paid;
        else
            return PaymentStatus.Unpaid;        
    }
    public string GetDeliveryStatus()
    {
        if (!Dispatched)
            return "Not dispatched";
        else
            return "Dispatched";    
    }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? Note { get; set; }
    public bool Dispatched { get; set; }
    public bool HasDelievery => !string.IsNullOrEmpty(Address!.State);
    [Column(TypeName = "jsonb")]
    public DeliveryAddress? Address { get; set; } = new();
    [Column(TypeName = "jsonb")]
    public List<ThirdPartyItem> ThirdPartyItems { get; set; } = [];
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; }
    public virtual ICollection<ProductOrderItem> ProductOrders { get; set; } = [];
    public virtual ICollection<ReturnedProduct> ReturnedProducts { get; set; } = [];
    public virtual ICollection<Payment> Payments { get; set; } = [];
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; } = new();    
    [ForeignKey(nameof(StoreId))]
    public virtual Store? Store { get; set; }
    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }
}
