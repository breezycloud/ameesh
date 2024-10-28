using Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers;

public class CartValidation
{
    [Required(ErrorMessage = "Cart cannot be empty")]
    public int? RowsCount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Discount { get; set; }
    public decimal AmountDue { get; set; }
    public decimal OtherService { get;set; }
    public decimal DeliveryAmt = 0M;
    public PaymentMode PaymentMode { get; set; } = PaymentMode.None;
}
