using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Reports;


public class SalesReportDto
{
    public Guid OrderId { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalOrderAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public decimal StoreSale { get; set; }
    public decimal StoreProfit { get; set; }
    public decimal ThirdPartyOrderAmount { get; set; }
}

public class SalesReportRequest
{
    public Guid StoreId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class SalesReportResponse
{
    public bool Success { get; set; }
    public string? StoreName { get; set; }
    public string? BranchAddress { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Criteria => string.IsNullOrWhiteSpace(EndDate) ? "Date" : "Range";
    public List<SaleRecord> Data { get; set; } = new();
    public List<ThirdPartySale> ThirdPartySales { get; set; } = new();

    public string Message { get; set; } = string.Empty;
    public SalesReportSummary Summary { get; set; } = new();
}

public class SalesReportSummary
{
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalSubTotal { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public decimal TotalAmountDue { get; set; }        
    public decimal TotalSales { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalTP { get; set; }
    public int TotalOrders { get; set; }
}


public class SaleRecord
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("storeid")]
    public Guid StoreId { get; set; }
    [Column("receiptno")]
    public string ReceiptNo { get; set; }
    [Column("date")]
    public DateTime Date { get; set; }
    [Column("customer")]
    public string? Customer { get; set; }
    [Column("totalamount")]
    public decimal TotalAmount { get; set; }
    [Column("discount")]
    public decimal Discount { get; set; }
    [Column("subtotal")]
    public decimal Subtotal { get; set; }
    [Column("amountpaid")]
    public decimal AmountPaid { get; set; }
    [Column("amountdue")]
    public decimal AmountDue { get; set; }
    [Column("storesale")]
    public decimal StoreSale { get; set; }
    [Column("storeprofit")]
    public decimal StoreProfit { get; set; }
    [Column("thirdpartyorderamount")]
    public decimal ThirdPartyOrderAmount { get; set; }
    [Column("status")]
    public string Status { get; set; }
}

public class ThirdPartySale
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string? ReceiptNo { get; set; }
    public DateOnly Date { get; set; } // or DateTime if using older EF Core
    public string? Customer { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public decimal StoreSale { get; set; }
    public decimal StoreProfit { get; set; }
    public decimal ThirdPartyOrderAmount { get; set; }
    public string? Status { get; set; }
}