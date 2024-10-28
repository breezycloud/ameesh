
using Shared.Enums;
using Shared.Models.Customers;

using Shared.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Orders;

public class OrderReferer
{
    [Required]
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? LabOrderId { get; set; }
    public Guid RefererId { get; set; }
    public Customer? Referer { get; set; }
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }
}
