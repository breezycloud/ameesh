namespace Shared.Models.Welfare;

public class SalaryBonusReportDto
{
    public string? Date { get; set; }
    public string Staff { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
}

public class SalaryBonusReportData
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public List<SalaryBonusReportDto> Data { get; set; } = [];

    public decimal TotalAmount => Data.Sum(x => x.Amount);

    public SalaryBonusReportData(ReportCriteria criteria, List<SalaryBonusReportDto> data)
    {
        Month = criteria.Month;
        Year = criteria.Year;
        Data = data;
    }
}
