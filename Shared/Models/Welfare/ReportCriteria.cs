using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Welfare;


public class ReportCriteria
{
    [Required]
    public string? ReportType { get; set; } = "Salary";

    [Required]
    public int? Month { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).Month;
    [Required]
    public int? Year { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow).Year;
}