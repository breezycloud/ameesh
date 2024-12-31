using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;
using Shared.Models.Orders;

namespace Shared.Models.Customers;

public class Customer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "Customer Name field is required!")]
    public string? CustomerName { get; set; }
    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone No must be atleast 11 digits")]
    public string? PhoneNo { get; set; }
    public string? ContactAddress { get; set; }
    [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone No must be atleast 11 digits")]
    public string? PhoneNo2 { get; set; }
    public string? ContactAddress2 { get; set; }
    public bool Regular { get; set; } = false;    
    public double Discount { get; set; }
    public double LoyaltyDiscount => TotalReferals >= 50 ? 0.05 : TotalReferals >= 100 ? 0.1 : 0;
    public double TotalDiscount => Discount + LoyaltyDiscount;
    public bool HasDiscount => TotalDiscount > 0 ? true : false;
    public int TotalReferals => Referrals.Count();
    [Column(TypeName = "decimal(18,2)")]
    public decimal StoreCredit { get; set; }
    public virtual List<Order> Orders { get; set; } = new();
    public virtual List<OrderReferer> Referrals { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; }
}