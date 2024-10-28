using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.Enums;
using Shared.Models.Users;

namespace Shared.Models.Orders;

public class Payment
{
    [Key]
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? PaymentDate { get; set; } = DateTime.Now;
    public PaymentMode PaymentMode { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }    
    public virtual Order? Order { get; set; }
    public virtual User? Cashier { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.Now;

}