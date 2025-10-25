namespace Shared.Models.Welfare;

public class PenaltyReportDto
{
    public string? Date { get; set; }
    public string Staff { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
}

public class PenaltyReportData
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public List<PenaltyReportDto> Data { get; set; } = [];

    public decimal TotalAmount => Data.Sum(x => x.Amount);

    public PenaltyReportData(ReportCriteria criteria, List<PenaltyReportDto> data)
    {
        Month = criteria.Month;
        Year = criteria.Year;
        Data = data;
    }
}
