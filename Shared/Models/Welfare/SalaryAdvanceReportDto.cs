namespace Shared.Models.Welfare;
public class SalaryAdvanceReportDto
{
    public string? Date { get; set; }
    public string Staff { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
}


public class SalaryAdvanceReportData
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public List<SalaryAdvanceReportDto> Data { get; set; } = [];

    public decimal TotalAmount => Data.Sum(x => x.Amount);

    public SalaryAdvanceReportData(ReportCriteria criteria, List<SalaryAdvanceReportDto> data)
    {
        Month = criteria.Month;
        Year = criteria.Year;
        Data = data;
    }
}