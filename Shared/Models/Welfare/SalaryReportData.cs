using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Welfare;


public class SalaryReportData
{    
    public SalaryReportData()
    {
        
    }
    public SalaryReportData(ReportCriteria criteria, List<SalaryReportDto> data)
    {
        Month = criteria.Month;
        Year = criteria.Year;
        Data = data;
    }
    public int? Month { get; set; }    
    public int? Year { get; set; }
    public List<SalaryReportDto> Data { get; set; } = [];
    public decimal SumSalaryAmount => Data.Sum(x => x.Amount);
    public decimal SumBonus => Data.Sum(x => x.Bonus);
    public decimal SumAdvance => Data.Sum(x => x.Advance);
    public decimal SumPenalty => Data.Sum(x => x.Penalty);
    public decimal SumEarnings => Data.Sum(x => x.Earnings);
    public decimal SumDeductions => Data.Sum(x => x.Deductions);
    public decimal SumTotal => Data.Sum(x => x.Total);

}

public class SalaryReportDto
{
    public string? Staff { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Bonus { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Earnings { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Advance { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Penalty { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Deductions { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; set; }
}