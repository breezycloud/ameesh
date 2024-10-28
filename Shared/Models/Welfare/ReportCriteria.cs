using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Welfare;


public class ReportCriteria
{
    [Required]
    public int? Month { get; set; }
    [Required]
    public int? Year { get; set; }
}