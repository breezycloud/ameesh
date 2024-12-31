using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shared.Enums;
using Shared.Models.Company;
using Shared.Models.Users;

namespace Shared.Models.Expenses;

public class Expense
{
    [Key]
    public Guid Id { get; set; }
    public Guid? UserId {  get; set; }
    public Guid StoreId { get; set; }
    public Guid TypeId { get; set; }
    public DateTime? Date {  get; set; }
    [Required]
    public string? Description { get; set; }    
    public string? Reference { get; set; }
    [Required(ErrorMessage = "Amount is required")]
    public decimal? Amount { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; }
    [ForeignKey(nameof(StoreId))]
    public Store? Store { get; set; }
    [ForeignKey(nameof(TypeId))]
    public ExpenseType? ExpenseType { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

}
