using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Expenses;
public record ExpenseReportFilter
{
    public Guid? id { get; set; } = null;
    [Required]
    public DateTime? from { get; set; } = DateTime.UtcNow;
    public DateTime? to { get; set; } 
    [Required]
    public string? type { get; set; } = "Date";
}