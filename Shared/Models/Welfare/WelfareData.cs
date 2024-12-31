using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Shared.Models.Welfare;

public class WelfareData
{
    public Guid Id { get; set; }    
    public Guid? UserId {get; set; } = null;
    public int Month { get; set; } = DateTime.UtcNow.Month;
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public WelfareType Type { get; set; }
    public string? StaffName { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Amount { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedDate { get ;set; }
    public DateTime? ModifiedDate { get; set; }
}