using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;
using Shared.Models.Orders;

namespace Shared.Models.Dashboard
{
    public class DashboardModel
    {
        public int TotalBranches { get; set; }
        public int TotalSales => TotalPharmacySales + TotalLabSales;
        public int TotalPharmacySales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpense { get; set; }
        public int TotalProductsSold { get; set; }
        public int TotalLabSales { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalEmployees { get; set; }
        public OrderChartModel[] ServicePieChart { get; set; } = [];
        public OrderChartModel[] ProductPieChart { get; set; } = [];
        public RecentOrder[] PharmacyRecentSales { get; set; } = [];
        public RecentOrder[] LabRecentSales { get; set; } = [];
        public List<MySeries> PharmacySeries { get; set; } = [];
        public OrderSalesLine[] PharmacySales { get; set; } = [];        
        public OrderSalesLine[] LabSales { get; set; } = [];
        public OrderSalesLine[] ProductSales { get; set; } = [];
        public BranchSalesChart[] BranchSales { get; set;  }  = []; 
        public BranchNetSalesChart[] BranchNetSales { get; set;  }  = []; 
        public Dictionary<string, int>? ServiceTopCustomer { get; set; }
        public Dictionary<string, int>? ProductTopCustomer { get; set; }
        public Earnings[]? Earnings { get; set; } = [];
        public Order? GroupedOrder = new();
        public GroupedEarnings[]? GroupedEarnings { get; set; } = [];
        public bool IsBusy { get; set; }
        public DateTime? Date { get; set; } = DateTime.Now;
        public SummaryByCashier[] SummaryByCashiers { get; set; } = [];
        public SummaryByPaymentMode[] SummaryByPaymentModes {get; set; } = [];
    }

    public class MySeries 
    {
        public int Year { get; set; }
        public OrderSalesLine[] Data { get; set; } = [];
    }
    public class OrderSalesLine
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int? Sales { get; set; }
    }
    public class OrderChartModel
    {
        public DateOnly Date { get; set; }
        public string? Item { get; set; }
        public int SalesCount { get; set; }
    }

    public class BranchSalesChart
    {
        public string? Branch { get; set; }
        public int PharmacySales { get; set; }
        public int LabSales { get; set; }
    }
    
    public class BranchNetSalesChart
    {
        public string? Branch { get; set; }
        public decimal PharmacySales { get; set; }
        public decimal LabSales { get; set; }
    }

    public class GroupedEarnings
    {
        public string? Item { get; set; }
        public int TotalAmount { get; set; }
    }

    public class Earnings
    {
        public DateOnly Date { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal SubTotal => TotalAmount - Discount;
        public decimal NetAmount => SubTotal - Refunds;
        public decimal Profit { get; set; }
        public decimal ActualProfit => Refunds > Profit ? Refunds - Profit : Profit - Refunds;
        public decimal Refunds { get; set; }
    }

    public class RecentOrder 
    {
        public DateOnly Date { get; set; }
        public bool IsHasDiscount { get; set; }
        public string? ReceiptNo { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Balance { get; set; }
        public bool HasOtherServices {get; set; }
        public bool HasOrderItems { get; set; } = false;
        public PaymentStatus PaymentStatus { get; set; }
        public string? DeliveryStatus { get; set; }
        public OrderStatus Status { get; set; }
    }

    public class ExpiringProducts
    {
        public string? Product { get; set; }
        public DateOnly Date { get; set; }
        public int Days { get; set; }
    }

    public class DashboardFilter
    {
        public DateOnly? Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }

    public class SummaryByCashier
    {
        public DateOnly Date {get; set; }
        public string? Cashier { get; set; }
        public decimal Amount { get; set; }
    }

    public class SummaryByPaymentMode
    {
        public DateOnly Date {get; set; }
        public string? Mode { get; set; }
        public decimal Amount { get; set; }
    }
}
