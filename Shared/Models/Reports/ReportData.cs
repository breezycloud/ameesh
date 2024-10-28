using Shared.Models.Company;
using Shared.Models.Customers;

using Shared.Models.Orders;
using Shared.Models.Users;

namespace Shared.Models.Reports;

public class ReportData
{
    public ReportHeader? ReportHeader { get; set; }
    public ReportFooter? ReportFooter { get; set; }
    public User? Employee { get; set; }
    public Store? Branch { get; set; }
    public Customer? Customer { get; set; }
    public Order? Order {get; set; }    
    public List<OrderCartRow>? Rows {get; set; }
    public string? PrevPage { get; set; }
    public string? QrCode { get; set; }
    public string? ReportType { get; set; }
    public byte[]? QR { get; set; }
    public byte[]? Logo { get; set; }

    
}