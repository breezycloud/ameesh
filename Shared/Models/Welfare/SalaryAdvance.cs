using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Models.Users;

namespace Shared.Models.Welfare;

public class SalaryAdvance
{
    public SalaryAdvance()
    {
        
    }
    public SalaryAdvance(WelfareData data)
    {
        Id = data.Id;
        UserId = data.UserId.GetValueOrDefault();
        Amount = data.Amount.GetValueOrDefault();
        CreatedDate = data.CreatedDate;
        ModifiedDate = data.ModifiedDate;
    }
    [Key]
    public Guid Id { get; set; }
    public int Month { get; set; } = DateTime.Now.Month;
    public int Year { get; set; } = DateTime.Now.Year;    
    public Guid UserId { get; set; }    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}