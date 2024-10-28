using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Expenses;

public class ExpenseType
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public string? Expense { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime ModifiedDate { get; set; }
    public ICollection<Expense> Expenses {  get; set; }  = new List<Expense>();
}
