using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Models.Users;

namespace Shared.Models.Welfare;


public class Salary
{
    public Salary()
    {
        
    }
    public Salary (WelfareData data)
    {
        Id = data.Id;
        UserId = data.UserId.GetValueOrDefault();
        Amount = data.Amount;
        CreatedDate = data.CreatedDate;
        ModifiedDate = data.ModifiedDate;
    }
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Month { get; set; } = DateTime.UtcNow.Month;
    public int Year { get; set; } = DateTime.UtcNow.Year;
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Amount { get; set; }    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Bonus => User?.SalaryBonus.OrderByDescending(x => x.Month).ThenByDescending(x=> x.Year).Sum(x => x.Amount) ?? 0;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Earnings => (Amount ?? 0) + Bonus;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Advance => User?.SalaryAdvances.OrderByDescending(x => x.Month).ThenByDescending(x=> x.Year).Sum(x => x.Amount) ?? 0;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Penalty => User?.Penalties.OrderByDescending(x => x.Month).ThenByDescending(x=> x.Year).Sum(x => x.Amount) ?? 0;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Deductions => Advance + Penalty;
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total => Earnings - Deductions;
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; } = null;
}