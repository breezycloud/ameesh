using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Enums;
using Shared.Models;
using Shared.Models.Dashboard;
using Shared.Models.Orders;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController(AppDbContext _context) : ControllerBase
{
    [HttpPost("filter")]
    public async Task<ActionResult<DashboardModel?>> GetDashboardData(DashboardFilter filter)
    {                
        int year = filter!.Date!.Value.Year;
        int month = filter!.Date!.Value.Month;
        int day = filter!.Date!.Value.Day;

        var date = new DateTime(year, month, day);
        DashboardModel? model = new()
        {
            Date = date,
            TotalPharmacySales = _context.Orders.AsParallel().Where(x => x.OrderDate == filter.Date).Count(),
            TotalProductsSold = _context.OrderItems.AsNoTracking().Include(x => x.Order).AsParallel().Where(x => x.Order!.OrderDate == filter.Date).GroupBy(x => x.ProductId).Count(),
            TotalExpense = _context.Expenses.AsParallel().Where(x => x.Date == date).Sum(x => x.Amount) ?? 0,            
            PharmacyRecentSales = _context.Orders.AsNoTracking()                                                
                                                .Include(x => x.ProductOrders)
                                                
                                                .Include(x => x.Payments)
                                                .AsParallel()
                                                .Where(x => x.OrderDate == filter!.Date)
                                                .OrderByDescending(x => x.ModifiedDate)
                                                .Take(10)
                                                .Select(x => new RecentOrder 
            {
                Date = x.OrderDate,
                ReceiptNo = x.ReceiptNo,
                Total = x.TotalAmount,                
                Status = x.Status,
                HasOrderItems = x.ProductOrders.Any(),
                Balance = x.Balance,
                SubTotal = x.SubTotal,
            }).ToArray(),
            ProductPieChart = _context.OrderItems.AsNoTracking()
                                                   .AsSplitQuery()
                                                   .Include(x => x.Order)
                                                   .Include(x => x.ProductData)
                                                   .AsParallel()
                                                   .Where(x => x.Order!.OrderDate == filter!.Date)
                                                   .GroupBy(x => x.ProductId)
                                                   .Select(p => new OrderChartModel
                                                   {
                                                        Date = DateOnly.FromDateTime(date),
                                                        Item = p.FirstOrDefault()!.Product,
                                                        SalesCount = p.Count()
                                                   }).OrderByDescending(x => x.SalesCount).Take(10).ToArray(),
            Earnings = _context.Orders.AsNoTracking()
                                        .Include(x => x.ProductOrders)
                                        .Include(x => x.ReturnedProducts)
                                        .Include(x => x.Payments)                                 
                                        .AsParallel()                                               
                                        .GroupBy(x => x.OrderDate)
                                        .AsEnumerable()                                        
                                        .Select(p => new Earnings
                                        {
                                            Date = p.Key,
                                            TotalSales = p.Count(),
                                            TotalAmount = p.Sum(s => s.TotalAmount),
                                            Discount = p.Sum(s => s.Discount),
                                            Profit = AppState.CalculateProfit(p.SelectMany(x => x.ProductOrders)),
                                            Refunds = AppState.CalculateRefunds(p.SelectMany(s => s.ReturnedProducts), p.First().OrderDate)
                                        }).OrderBy(x => x.Date).ToArray(),
            SummaryByCashiers = _context.Payments.AsNoTracking().AsSplitQuery().Include(x => x.Cashier).Where(x => x.PaymentDate!.Value == date).GroupBy(x => x.UserId).Select(s => new SummaryByCashier
            {
                Date = DateOnly.FromDateTime(date),
                Cashier = s.FirstOrDefault()!.Cashier!.ToString(),
                Amount = s.Sum(x => x.Amount)
            }).OrderByDescending(x => x.Amount).ToArray(),
            SummaryByPaymentModes = _context.Payments.AsNoTracking().AsParallel().Where(x => x.PaymentDate!.Value.Date == date).GroupBy(x => x.PaymentMode).Select(s => new SummaryByPaymentMode
            {
                Date = DateOnly.FromDateTime(date),
                Mode = s!.Key.ToString(),
                Amount = s.Sum(x => x.Amount)
            }).Where(x => x.Amount > 0).OrderByDescending(x => x.Amount).ToArray()

        };
        model.TotalRevenue = model.Earnings.Where(x => x.Date == filter.Date).Select(x => x.ActualProfit).FirstOrDefault(0M);
        var months = Enumerable.Range(1, 12);
        var sales = await _context.Orders.GroupBy(x => new { x.OrderDate.Year, x.OrderDate.Month }).Select(x => new OrderSalesLine
        {
            Year = x.Key.Year,
            Month = x.Key.Month,
            Sales = x.Count(),
        }).ToListAsync();
        List<OrderSalesLine>? lines = [];
        foreach (var sale in sales.GroupBy(x => x.Year))
        {                        
            for (int i = 1; i <= months.Count(); i++)
            {
                var akwai = sales.FirstOrDefault(x => x.Month == i);
                if (akwai is null)
                {
                    lines.Add(new OrderSalesLine
                    {  
                        Year = sale.Key,
                        Month = i,
                        Sales  = 0
                    });
                }
                else
                {
                    lines.Add(new OrderSalesLine
                    {  
                        Year = sale.Key,
                        Month = i,
                        Sales  = akwai.Sales
                    });
                }

            }
        }
        foreach (var item in lines!.AsParallel().GroupBy(x => x.Year))
        {
            var series = lines.Where(x => x.Year == item.Key).OrderBy(p => p.Month).ThenBy(p => p.Year).ToArray();
            model.PharmacySeries.Add(new MySeries
            {
                Year = item.Key,
                Data = series
            });
        }
        return model;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardModel?>> GetDashboardData()
    {
        DashboardModel? model = new()
        {
            TotalPharmacySales = _context.Orders.AsParallel().Count(),
            TotalCustomers = _context.Customers.AsParallel().Count(),
            //
            BranchSales = await _context.Stores.AsNoTracking().AsSplitQuery().Include(x => x.Orders).GroupBy(x => x.Id).Select(x => new BranchSalesChart
            {
                Branch = x.FirstOrDefault(y => y.Id == x.Key)!.BranchName,
                PharmacySales = x.SelectMany(y => y.Orders).Count()
            }).ToArrayAsync(),            
        
            PharmacyRecentSales = await _context.Orders.AsNoTracking()
                                                .Include(x => x.ProductOrders)
                                                
                                                .Include(x => x.Payments)
                                                .OrderByDescending(x => x.OrderDate)
                                                .Take(10)
                                                .Select(x => new RecentOrder 
            {
                Date = x.OrderDate,
                ReceiptNo = x.ReceiptNo,
                Total = x.TotalAmount,                
                Status = x.Status,
                HasOrderItems = x.ProductOrders.Any(),
                Balance = x.Balance,
                SubTotal = x.SubTotal,
            }).ToArrayAsync(),
        
            ProductPieChart = await _context.OrderItems.AsNoTracking()
                                                   .AsSplitQuery()
                                                   .Include(x => x.ProductData)
                                                   .GroupBy(x => x.ProductId)
                                                   .Select(p => new OrderChartModel
                                                   {
                                                        Item = p.FirstOrDefault()!.Product,
                                                        SalesCount = p.Count()
                                                   }).OrderByDescending(x => x.SalesCount).Take(10).ToArrayAsync(),
        


        };
        // Define the range of months you want to include
        int year = 2024;
        var monthsInYear = Enumerable.Range(1, 12);
        model.PharmacySales =
                (from month in monthsInYear
                join order in _context.Orders.AsNoTracking().AsParallel()
                    on new { Month = month, Year = year } equals new { Month = order.OrderDate.Month, Year = order.OrderDate.Year }
                    into monthOrders
                from mo in monthOrders.DefaultIfEmpty()
                group mo by new { Month = month, Year = year } into g
                select new OrderSalesLine
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Sales = g.Count(o => o != null)  // Count non-null orders for each month
                }).OrderBy(x => x.Year).ThenBy(x => x.Month).ToArray();        

        // var PharmacySales = await _context.Orders.AsNoTracking().GroupBy(x => new {x.OrderDate.Month, x.OrderDate.Year}).Select(p => new OrderSalesLine
        // {
        //     Year = p.Key.Year,
        //     Month = p.Key.Month,
        //     Sales = p.Count()
        // }).ToArrayAsync();
        // foreach (var item in PharmacySales!.AsParallel().GroupBy(x => x.Year))
        // {
        //     var series = PharmacySales.Where(x => x.Year == item.Key).OrderBy(p => p.Month).ThenBy(p => p.Year).ToArray();
        //     model.PharmacySeries.Add(new MySeries
        //     {
        //         Year = item.Key,
        //         Data = series
        //     });
        // }
        model.Earnings = _context.Orders.AsNoTracking()
                                        .Include(x => x.ProductOrders)
                                        .Include(x => x.ReturnedProducts)
                                        .Include(x => x.Payments)                                 
                                        .AsParallel()       
                                        .Where(x => x.Status == OrderStatus.Completed)
                                        .GroupBy(x => x.OrderDate)
                                        .AsEnumerable()                                        
                                        .Select(p => new Earnings
                                        {
                                            Date = p.Key,
                                            TotalSales = p.Count(),
                                            TotalAmount = p.Sum(s => s.TotalAmount),
                                            Discount = p.Sum(s => s.Discount),
                                            Profit = AppState.CalculateProfit(p.SelectMany(x => x.ProductOrders)),
                                            Refunds = p.SelectMany(s => s.ReturnedProducts).Sum(s => s.Cost.GetValueOrDefault())
                                        }).OrderBy(x => x.Date).ToArray();
        
        return model;        
    }    
}
