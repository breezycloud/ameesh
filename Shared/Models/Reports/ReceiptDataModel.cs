using Shared.Models.Customers;

namespace Shared.Models.Reports;

public class ReportDataModel
{
    public Customer? Customer { get; set; }
    public List<ReceiptItem> ReceipItems { get; set; } = new();
}