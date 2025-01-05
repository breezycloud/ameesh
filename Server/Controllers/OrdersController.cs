using System.Reflection.Metadata;
using EFCore.BulkExtensions;
using QuestPDF.Fluent;
using HyLook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Context;
using Server.Pages.Reports.Templates.Sales;
using Shared.Enums;
using Shared.Helpers;
using Shared.Models;
using Shared.Models.Orders;
using Shared.Models.Reports;
using Server.Pages.Reports.Templates.Receipt;

namespace Server.Controllers;

//[Authorize]
[Route("api/[controller]")]
[ApiController]
//[ResponseCache(CacheProfileName = "Default60")]
public class OrdersController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly ILogger<OrdersController> _logger;

	public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
	{
		_context = context;
		_logger = logger;
	}

    [HttpPost("report")]
    public async Task<ActionResult<SalesReportTemplate>> Report(ReportFilter filter, CancellationToken cancellationToken)
    {
        List<SaleItem> items = [];
        var store = _context.Stores.AsParallel().FirstOrDefault(x => x.Id == filter.StoreID);
        if (store is null)
            return new SalesReportTemplate();

        List<Order> Orders = [];
        if (filter.Criteria == "Date")
        {
            if (filter.Option == "All")
            {
                Orders = _context.Orders.AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(x => x.User)
                                            .Include(x => x.Customer)                                            
                                            .Include(x => x.ProductOrders)
                                            .ThenInclude(x => x.ProductData)
                                            .Include(x => x.ReturnedProducts.Where(d => d.Date.Date == filter!.StartDate.GetValueOrDefault().Date))
                                            .Include(x => x.Payments)
                                            .ThenInclude(x=>x.Cashier)
                                            .AsParallel()
                                            .AsEnumerable()
                                            .Where(x =>  x.StoreId == filter.StoreID && x.OrderDate == DateOnly.FromDateTime(filter!.StartDate!.Value))
                                            .OrderByDescending(x => x.ModifiedDate)
                                            .ToList();
            }             
            else    
            {
                Orders = _context.Payments.AsNoTracking()
                                 .Include(x => x!.Order)
                                 .ThenInclude(x => x!.Customer)
                                 .Include(x => x.Order)
                                 .ThenInclude(x => x!.ProductOrders)
                                 .ThenInclude(x => x!.ProductData)
                                 .Include(x => x!.Order)
                                 .ThenInclude(x => x!.ReturnedProducts.Where(d => d.Date.Date == filter!.StartDate.GetValueOrDefault().Date))
                                 .Include(x => x!.Order)                                 
                                 .Include(x => x!.Order)                                 
                                 .AsParallel()
                                 .Where(x => x.UserId == filter!.UserID && x.PaymentDate.GetValueOrDefault().Date == filter!.StartDate!.Value.Date)
                                 .AsEnumerable()
                                 .Select(x => x!.Order)
                                 .Where(x => x.Balance <= 0)
                                 .OrderByDescending(x => x.ModifiedDate)
                                 .ToList();                
            }
        }
        else
        {
            if (filter.Option == "All")
            {
                Orders = _context.Orders.AsNoTracking()
                                .AsSplitQuery()
                                .Include(x => x.User)
                                .Include(x => x.Customer)
                                .Include(x => x.ProductOrders)
                                .ThenInclude(x => x.ProductData)
                                .Include(x => x.ReturnedProducts.Where(d => d.Date.Date >= filter!.StartDate.GetValueOrDefault().Date && d.Date.Date >= filter!.EndDate.GetValueOrDefault().Date))
                                .Include(x => x.Payments)
                                .ThenInclude(x => x.Cashier)
                                .AsParallel()
                                .AsEnumerable()
                                .Where(x => x.StoreId == filter.StoreID && x.OrderDate >= DateOnly.FromDateTime(filter!.StartDate!.Value) && x.OrderDate <= DateOnly.FromDateTime(filter!.EndDate!.Value))
                                .OrderByDescending(x => x.ModifiedDate)
                                .ToList();
            }            
            else
            {
                Orders = _context.Payments.AsNoTracking()
                                 .Include(x => x!.Order)
                                 .ThenInclude(x => x!.Customer)
                                 .Include(x => x.Order)
                                 .ThenInclude(x => x!.ProductOrders)
                                 .ThenInclude(x => x!.ProductData)
                                 .Include(x => x!.Order)
                                 .ThenInclude(x => x!.ReturnedProducts.Where(d => d.Date.Date >= filter!.StartDate.GetValueOrDefault().Date && d.Date.Date >= filter!.EndDate.GetValueOrDefault().Date))
                                 .Include(x => x!.Order)                                 
                                 .Include(x => x!.Order)                                 
                                 .AsParallel()
                                 .Where(x => x.UserId == filter!.UserID && x.PaymentDate.GetValueOrDefault().Date >= filter!.StartDate!.Value.Date && x.PaymentDate.GetValueOrDefault().Date <= filter!.EndDate!.Value.Date)
                                 .AsEnumerable()
                                 .Select(x => x!.Order) 
                                 .Where(x => x.Balance <= 0)
                                 .OrderByDescending(x => x.ModifiedDate)
                                 .ToList();
            }
        }
        if (!Orders.Any())
            return new SalesReportTemplate
            {
                StoreName = store!.BranchName,
                BranchAddress = store!.BranchAddress,
                Criteria = filter.Criteria,
                StartDate = filter.StartDate!.Value.ToString("dd/MM/yyyy"),
                EndDate = filter!.EndDate is not null ? filter.EndDate!.Value.ToString("dd/MM/yyyy") : "",
            };

        
        var Report = new SalesReportTemplate
        {
            StoreName = store!.BranchName,
            BranchAddress = store!.BranchAddress,
            Criteria = filter.Criteria,
            StartDate = filter.StartDate!.Value.ToString("dd/MM/yyyy"),
            EndDate =  filter!.EndDate is not null ? filter.EndDate!.Value.ToString("dd/MM/yyyy") : "",
        };
         

        foreach (var x in Orders.AsParallel())
        {
            var saleItem = new SaleItem
            {
                Id = x.Id,
                Date = x.OrderDate,
                Option = "Store",
                Customer = x.Customer!.CustomerName,
                IsHasDiscount = x.Customer!.HasDiscount,
                ReceiptNo = x.ReceiptNo,
                AmountPaid = x.Payments.Sum(p => p.Amount),
                // saleItem.Mode = x.Payments!.LastOrDefault()!.PaymentMode;
                TotalAmount = x.TotalAmount,
                SubTotal = x.SubTotal,
                Balance = x.Balance,
                BuyPrice = x.ProductOrders.Sum(x => x.BuyPrice) + x.ThirdPartyItems.Sum(x => x.Cost),
                SellPrice = x.ProductOrders.Sum(x => x.Cost) + x.ThirdPartyItems.Sum(x => x.Total),
                Profit = AppState.CalculateProfit(x.ProductOrders) + AppState.CalculateProfit(x.ThirdPartyItems) - x.ReturnedProducts.Sum(s => s.Cost.GetValueOrDefault()),            
                Quantity = x.ProductOrders.Sum(p => p.Quantity) + (int)x.ThirdPartyItems.Sum(x => x.Quantity),
                HasReturns = x.ReturnedProducts.Any(p => p.Quantity > 0),
                ReturnsQty = x.ReturnedProducts.Sum(p => p.Quantity.GetValueOrDefault()),
                HasOrderItems = x.ProductOrders.Any() || x.ThirdPartyItems.Any(),
                Refund = x.ReturnedProducts.Sum(r => (decimal)r.Quantity! * r.Cost.GetValueOrDefault()),
                Discount = x.Discount,
                // Sale = x.User!.ToString(),
                // Dispenser = x.DispensedBy!.ToString()
            };

            //saleItem.Cashier = x.Payments.AsEnumerable().LastOrDefault()!.Cashier!.ToString();
            Report.SaleItems.Add(saleItem);
        }
        var report = new SalesReport(Report);
        var pdf = report.GeneratePdf();
        return File(pdf, "application/pdf", "your_pdf_filename.pdf");
    }

    [HttpGet("paymentstatus/{id}")]
    public async Task<bool> GetPaymentStatus(Guid id)
    {
        try
        {
            var order = await _context.Orders.Where(x => x.Id == id).FirstOrDefaultAsync();
            return order!.Balance > 0 ? false : true;
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    [HttpPost("completeorder")]
    public async Task<ActionResult> CompleteOrder(CompleteBill bill)
    {
        try
        {
            await _context.Orders.Where(x => x.Id == bill.OrderId).ExecuteUpdateAsync(s => s.SetProperty(p => p.PaymentConfirmed, true));   
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex);
        }        
        return Ok();
    }

    [HttpPost("dispatch")]
    public async Task<ActionResult> DispatchOrder(CompleteBill bill)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        try
        {
            var order = await _context.Orders.FindAsync(bill.OrderId);
            if (order is null)
             return NotFound();

            order!.Dispatched = true;
            order!.Address!.DispatchedDate = date;
            order!.ModifiedDate = DateTime.UtcNow;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex);
        }        
        return Ok();
    }

    [HttpPost("delivered")]
    public async Task<ActionResult> DeliverOrder(CompleteBill bill)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        try
        {
            var order = await _context.Orders.FindAsync(bill.OrderId);
            if (order is null)
             return NotFound();

            order!.Delivered = true;
            order!.Address!.DeliveryDate = date;
            order!.ModifiedDate = DateTime.UtcNow;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex);
        }        
        return Ok();
    }

    [HttpPost("complete")]
    public async Task<ActionResult> CompleteOrder(Order x)
    {
        try
        {
            var admin = _context.Users.AsParallel().Where(x => x.Role == UserRole.Master).FirstOrDefault();
            x.Status = OrderStatus.Completed;
            _context.Entry(x).State = EntityState.Modified;                        
            var item = new
            {
                id = x.Id,
                storeId = x.StoreId,
                items = x.ProductOrders.Select(t => new { productId = t.ProductId, stockId = t.StockId, qty = t.Quantity }).ToList(),
                remainingTime = DateTime.UtcNow.Subtract(x.CreatedDate).Hours
            };
            foreach (var row in item.items)
            {
                var product = await _context.Products.Where(x => x.StoreId == item.storeId && x.Id == row.productId).FirstOrDefaultAsync();
                if (product is null)
                    continue;

                if (!product!.Dispensary.Any(x => x.id == row.stockId))
                    continue;
                product!.Dispensary.FirstOrDefault(x => x.id == row.stockId)!.Quantity -= row.qty;
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await _context.OrderItems.Where(x => x.OrderId == item.id && x.ProductId == row.productId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, Shared.Enums.OrderStatus.Completed));
            }            
            await _context.SaveChangesAsync();
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex);
        }        
        return Ok();
    }

	[HttpPost("paged")]
	public async Task<ActionResult<GridDataResponse<OrderWithData>?>> PagedOrders(PaginationParameter parameter, CancellationToken cancellationToken)
	{
        GridDataResponse<OrderWithData>? response = new();

        if (!string.IsNullOrEmpty(parameter.SearchTerm))
        {
            var pattern = $"%{parameter.SearchTerm}%";
            response!.Data = _context.Orders.AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(x => x.User)                                            
                                            .Include(x => x.Customer)
                                            .Include(x => x.Store)
                                            .Include(x => x.ProductOrders)
                                            .Include(x => x.Payments)
                                            .ThenInclude(x => x.Cashier)
                                            .AsEnumerable()
                                            .AsParallel()
                                            .Where(x => x.StoreId == parameter.FilterId
                                            && x.Customer!.CustomerName!.Contains(parameter.SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                                            x.ReceiptNo!.Contains(parameter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                            (parameter.HasThirdItems && x.ThirdPartyItems.Any()) ||
                                            ((x.Address is not null && !string.IsNullOrEmpty(x.Address!.State)) && x.Address!.State!.Contains(parameter.SearchTerm, StringComparison.OrdinalIgnoreCase)))
                                            .OrderByDescending(x => x.ModifiedDate)
                                            .Skip(parameter.Page)
                                            .Take(parameter.PageSize)
                                            .Select(x => new OrderWithData
                                            {
                                                Id = x.Id,
                                                Date = x.OrderDate,
                                                StoreName = x.Store!.BranchName,
                                                CustomerName = x.Customer!.CustomerName,
                                                IsHasDiscount = x.Customer!.HasDiscount,
                                                OrderType = "Store",
                                                ReceiptNo = x.ReceiptNo,
                                                TotalAmount = x.TotalAmount,
                                                PaymentStatus = x.GetPaymentStatus().ToString(),
                                                DeliveryStatus = x.GetDeliveryStatus(),
                                                HasDelievery = x.HasDelievery,
                                                Dispatched = x.Dispatched,
                                                Delivered = x.Delivered,
                                                OrderStatus = x.Status,
                                                Balance = x.Balance,
                                                SubTotal = x.SubTotal,
                                                HasReturns = x.ReturnedProducts.Any(p => p.Quantity > 0),
                                                HasOrderItems = x.ProductOrders.Any(),
                                                Discount = x.Discount,
                                                Sale = x.User!.ToString(),
                                                Cashier = x.Payments.AsEnumerable().LastOrDefault()!.Cashier!.ToString(),
                                                CreatedDate = x.CreatedDate,
                                                ModifiedDate = x.ModifiedDate
                                            }).ToList();
            response!.TotalCount =  _context.Orders.Include(x => x.Customer).AsEnumerable().AsParallel().Where(x => x.StoreId == parameter.FilterId
                                            && x.Customer!.CustomerName!.Contains(parameter.SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                                            x.ReceiptNo!.Contains(parameter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                            (parameter.HasThirdItems && x.ThirdPartyItems.Any()) ||
                                            ((x.Address is not null && !string.IsNullOrEmpty(x.Address!.State)) && x.Address!.State!.Contains(parameter.SearchTerm, StringComparison.OrdinalIgnoreCase))).Count();
        }
        else
        {
            response!.Data =  _context.Orders.AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(x => x.User)                                            
                                            .Include(x => x.Customer)
                                            .Include(x => x.Store)
                                            .Include(x => x.ProductOrders)
                                            .Include(x => x.Payments)
                                            .ThenInclude(x => x.Cashier)
                                            .AsEnumerable()
                                            .AsParallel()
                                            .Where(x =>  (parameter.HasThirdItems ? (x.StoreId == parameter.FilterId && x.ThirdPartyItems.Any()) : x.StoreId == parameter.FilterId))
                                            .OrderByDescending(x => x.ModifiedDate)
                                            .Skip(parameter.Page)
                                            .Take(parameter.PageSize)
                                            .Select(x => new OrderWithData
                                            {
                                                Id = x.Id,
                                                Date = x.OrderDate,
                                                StoreName = x.Store!.BranchName,
                                                CustomerName = x.Customer!.CustomerName,
                                                IsHasDiscount = x.Customer!.HasDiscount,
                                                OrderType = "Store",
                                                ReceiptNo = x.ReceiptNo,
                                                HasReturns = x.ReturnedProducts.Any(p => p.Quantity > 0),
                                                HasOrderItems = x.ProductOrders.Any(),
                                                TotalAmount = x.TotalAmount,
                                                SubTotal = x.SubTotal,
                                                PaymentStatus = x.GetPaymentStatus().ToString(),
                                                DeliveryStatus = x.GetDeliveryStatus(),
                                                HasDelievery = x.HasDelievery,
                                                Dispatched = x.Dispatched,
                                                Delivered = x.Delivered,
                                                OrderStatus = x.Status,
                                                Balance = x.Balance,
                                                Discount = x.Discount,
                                                Sale = x.User!.ToString(),
                                                Cashier = x.Payments.AsEnumerable().LastOrDefault()!.Cashier!.ToString(),
                                                CreatedDate = x.CreatedDate,
                                                ModifiedDate = x.ModifiedDate
                                            }).ToList();
            response!.TotalCount =  _context.Orders.AsEnumerable().AsParallel().Where(x =>  (parameter.HasThirdItems ? (x.StoreId == parameter.FilterId && x.ThirdPartyItems.Any()) : x.StoreId == parameter.FilterId)).Count();
        }        
        return response;
	}
    [HttpPost("cancelorders")]
    public async Task<ActionResult> CancelOrder(Guid id)
    {
        var order = await _context.Orders.AsNoTracking().AsSplitQuery().Include(c => c.Customer).Include(x => x.ProductOrders).FirstOrDefaultAsync(x => x.Id == id);
        if (order is null)
        {
            return Ok();
        }
        try
        {
            foreach (var item in order.ProductOrders)
            {
                var product = await _context.Products.Where(x => x.StoreId == order.StoreId && x.Id == item.ProductId).FirstOrDefaultAsync();
                if (product is null)
                    continue;

                if (!product!.Dispensary.Any(x => x.id == item.StockId))
                    continue;
                product!.Dispensary.FirstOrDefault(x => x.id == item.StockId)!.Quantity += item.Quantity;
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await _context.OrderItems.Where(x => x.OrderId == order.Id && x.ProductId == product.Id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, Shared.Enums.OrderStatus.Canceled));
            }
            await _context.Orders.Where(x => x.Id == order.Id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, Shared.Enums.OrderStatus.Canceled));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return Ok();

    }

    [HttpPost("withreturns/paged")]
    public async Task<ActionResult<GridDataResponse<OrderWithData>?>> PagedReturnOrders(PaginationParameter parameter, CancellationToken cancellationToken)
    {
        GridDataResponse<OrderWithData>? response = new();

        if (!string.IsNullOrEmpty(parameter.SearchTerm))
        {
            var pattern = $"%{parameter.SearchTerm}%";
            response!.Data =  _context.Orders.AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(x => x.User)                                            
                                            .Include(x => x.Customer)
                                            .Include(x => x.Store)
                                            .Include(x => x.ProductOrders)
                                            .Include(x => x.ReturnedProducts.Any(r => r.Quantity > 0))
                                            .Include(x => x.Payments)
                                            .ThenInclude(x => x.Cashier)
                                            .Where(x => x.StoreId == parameter.FilterId
                                                && EF.Functions.ILike(x.Customer!.CustomerName!, pattern) || EF.Functions.ILike(x.ReceiptNo!, pattern))
                                            .AsParallel()
                                            .OrderByDescending(x => x.ModifiedDate)
                                            .Skip(parameter.Page)
                                            .Take(parameter.PageSize)
                                            .Select(x => new OrderWithData
                                            {
                                                Id = x.Id,
                                                Date = x.OrderDate,
                                                StoreName = x.Store!.BranchName,
                                                CustomerName = x.Customer!.CustomerName,
                                                IsHasDiscount = x.Customer!.HasDiscount,
                                                OrderType = "Store",
                                                ReceiptNo = x.ReceiptNo,
                                                TotalAmount = x.TotalAmount,
                                                PaymentStatus = x.GetPaymentStatus().ToString(),
                                                DeliveryStatus = x.GetDeliveryStatus(),
                                                HasDelievery = x.HasDelievery,
                                                Dispatched = x.Dispatched,
                                                Delivered = x.Delivered,
                                                OrderStatus = x.Status,
                                                Balance = x.Balance,
                                                SubTotal = x.SubTotal,
                                                Discount = x.Discount,

                                                Sale = x.User!.ToString(),
                                                
                                                Cashier = x.Payments.AsEnumerable().LastOrDefault()!.Cashier!.ToString(),
                                                CreatedDate = x.CreatedDate,
                                                ModifiedDate = x.ModifiedDate
                                            }).ToList();
            response!.TotalCount = await _context.Orders.Where(x => x.StoreId == parameter.FilterId
            && EF.Functions.ILike(x.Customer!.CustomerName!, pattern) || EF.Functions.ILike(x.ReceiptNo!, pattern)).CountAsync(cancellationToken);
        }
        else
        {
            response!.Data = _context.Orders.AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(x => x.User)                                            
                                            .Include(x => x.Customer)
                                            .Include(x => x.Store)
                                            .Include(x => x.ProductOrders)
                                            .Include(x => x.ReturnedProducts.Any(r => r.Quantity > 0))
                                            .Include(x => x.Payments)
                                            .ThenInclude(x => x.Cashier)
                                            .Where(store => store.StoreId == parameter.FilterId)
                                            .AsParallel()
                                            .OrderByDescending(x => x.ModifiedDate)
                                            .Skip(parameter.Page)
                                            .Take(parameter.PageSize)
                                            .Select(x => new OrderWithData
                                            {
                                                Id = x.Id,
                                                Date = x.OrderDate,
                                                StoreName = x.Store!.BranchName,
                                                CustomerName = x.Customer!.CustomerName,
                                                IsHasDiscount = x.Customer!.HasDiscount,
                                                OrderType = "Store",
                                                ReceiptNo = x.ReceiptNo,
                                                TotalAmount = x.TotalAmount,
                                                SubTotal = x.SubTotal,
                                                PaymentStatus = x.GetPaymentStatus().ToString(),
                                                DeliveryStatus = x.GetDeliveryStatus(),
                                                HasDelievery = x.HasDelievery,
                                                Dispatched = x.Dispatched,
                                                Delivered = x.Delivered,
                                                OrderStatus = x.Status,
                                                Balance = x.Balance,
                                                Discount = x.Discount,

                                                Sale = x.User!.ToString(),
                                                Cashier = x.Payments.AsEnumerable().LastOrDefault()!.Cashier!.ToString(),
                                                CreatedDate = x.CreatedDate,
                                                ModifiedDate = x.ModifiedDate
                                            }).ToList();
            response!.TotalCount = await _context.Orders.Where(store => store.StoreId == parameter.FilterId).CountAsync(cancellationToken);
        }
        return response;
    }

    [HttpGet]
	public async Task<ActionResult<IEnumerable<Order>>> GetCategories()
	{
		return await _context.Orders.OrderByDescending(x => x.CreatedDate).ToArrayAsync();
	}

    [HttpGet("thirdparty")]
    public async Task<ActionResult<IEnumerable<OrderWithThirdParty>>> GetThirdPartyOrders(ReportFilter filter)
    {
        
        if (filter.Criteria == "Date")
        {
            return _context.Orders.AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(x => x.User)
                                            .Include(x => x.Customer)                                            
                                            .Include(x => x.ProductOrders)
                                            .ThenInclude(x => x.ProductData)
                                            .Include(x => x.ReturnedProducts.Where(d => d.Date.Date == filter!.StartDate.GetValueOrDefault().Date))
                                            .Include(x => x.Payments)
                                            .ThenInclude(x=>x.Cashier)
                                            .AsParallel()
                                            .AsEnumerable()
                                            .Where(x =>  x.StoreId == filter.StoreID && x.OrderDate == DateOnly.FromDateTime(filter!.StartDate!.Value))
                                            .OrderByDescending(x => x.ModifiedDate)
                                            .Select(x => new OrderWithThirdParty
                                            {
                                                 Id = x.Id,
                                                Date = x.OrderDate,
                                                Option = "Store",
                                                Customer = x.Customer!.CustomerName,
                                                IsHasDiscount = x.Customer!.HasDiscount,
                                                ReceiptNo = x.ReceiptNo,
                                                AmountPaid = x.Payments.Sum(p => p.Amount),
                                                TotalAmount = x.TotalAmount,
                                                SubTotal = x.SubTotal,
                                                Balance = x.Balance,
                                                BuyPrice = x.ProductOrders.Sum(x => x.BuyPrice) + x.ThirdPartyItems.Sum(x => x.Cost),
                                                SellPrice = x.ProductOrders.Sum(x => x.Cost) + x.ThirdPartyItems.Sum(x => x.Total),
                                                Profit = AppState.CalculateProfit(x.ProductOrders) + AppState.CalculateProfit(x.ThirdPartyItems) - x.ReturnedProducts.Sum(s => s.Cost.GetValueOrDefault()),            
                                                Quantity = x.ProductOrders.Sum(p => p.Quantity) + (int)x.ThirdPartyItems.Sum(x => x.Quantity),
                                                HasReturns = x.ReturnedProducts.Any(p => p.Quantity > 0),
                                                ReturnsQty = x.ReturnedProducts.Sum(p => p.Quantity.GetValueOrDefault()),
                                                HasOrderItems = x.ProductOrders.Any() || x.ThirdPartyItems.Any(),
                                                Refund = x.ReturnedProducts.Sum(r => (decimal)r.Quantity! * r.Cost.GetValueOrDefault()),
                                                Discount = x.Discount,
                                                HasThirdItems = x.ThirdPartyItems.Any(),
                                                ThirdPartyItems = x.ThirdPartyItems,
                                                OrderItems = x.ProductOrders.Select(p => new OrderItemDetail
                                                {
                                                    ItemName = p.Product,
                                                    Quantity = p.Quantity,
                                                    Cost =  p.Cost,
                                                    Consultation = p.BuyPrice
                                                }).ToArray()
                                            
                                            }).ToList();
        }
        else
        {
             return _context.Orders.AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(x => x.User)
                                            .Include(x => x.Customer)                                            
                                            .Include(x => x.ProductOrders)
                                            .ThenInclude(x => x.ProductData)
                                            .Include(x => x.ReturnedProducts.Where(d => d.Date.Date == filter!.StartDate.GetValueOrDefault().Date))
                                            .Include(x => x.Payments)
                                            .ThenInclude(x=>x.Cashier)
                                            .AsParallel()
                                            .AsEnumerable()
                                            .Where(x =>  x.StoreId == filter.StoreID && x.OrderDate >= DateOnly.FromDateTime(filter!.StartDate!.Value) 
                                            && x.OrderDate <= DateOnly.FromDateTime(filter!.EndDate!.Value))
                                            .OrderByDescending(x => x.ModifiedDate)
                                            .Select(x => new OrderWithThirdParty
                                            {
                                                 Id = x.Id,
                                                Date = x.OrderDate,
                                                Option = "Store",
                                                Customer = x.Customer!.CustomerName,
                                                IsHasDiscount = x.Customer!.HasDiscount,
                                                ReceiptNo = x.ReceiptNo,
                                                AmountPaid = x.Payments.Sum(p => p.Amount),
                                                TotalAmount = x.TotalAmount,
                                                SubTotal = x.SubTotal,
                                                Balance = x.Balance,
                                                BuyPrice = x.ProductOrders.Sum(x => x.BuyPrice) + x.ThirdPartyItems.Sum(x => x.Cost),
                                                SellPrice = x.ProductOrders.Sum(x => x.Cost) + x.ThirdPartyItems.Sum(x => x.Total),
                                                Profit = AppState.CalculateProfit(x.ProductOrders) + AppState.CalculateProfit(x.ThirdPartyItems) - x.ReturnedProducts.Sum(s => s.Cost.GetValueOrDefault()),            
                                                Quantity = x.ProductOrders.Sum(p => p.Quantity) + (int)x.ThirdPartyItems.Sum(x => x.Quantity),
                                                HasReturns = x.ReturnedProducts.Any(p => p.Quantity > 0),
                                                ReturnsQty = x.ReturnedProducts.Sum(p => p.Quantity.GetValueOrDefault()),
                                                HasOrderItems = x.ProductOrders.Any() || x.ThirdPartyItems.Any(),
                                                Refund = x.ReturnedProducts.Sum(r => (decimal)r.Quantity! * r.Cost.GetValueOrDefault()),
                                                Discount = x.Discount,
                                                HasThirdItems = x.ThirdPartyItems.Any(),
                                                ThirdPartyItems = x.ThirdPartyItems,
                                                OrderItems = x.ProductOrders.Select(p => new OrderItemDetail
                                                {
                                                    ItemName = p.Product,
                                                    Quantity = p.Quantity,
                                                    Cost =  p.Cost,
                                                    Consultation = p.BuyPrice
                                                }).ToArray()
                                            
                                            }).ToList();
        }

    }

	[HttpGet("{id}")]
	public async Task<ActionResult<Order?>> GetOrder(Guid id)
	{
		if (_context.Orders == null)
		{
			return NotFound();
		}
		var order = await _context.Orders.AsNoTracking()
                                      .AsSplitQuery()
                                      .Include(x => x.User)
                                      .Include(x => x.Customer)
                                      .Include(x => x.ProductOrders)
                                      .ThenInclude(x => x.ProductData)
                                      .ThenInclude(x => x!.Store)
                                      .Include(x => x.User)
                                      .Include(x => x.Store)
                                      .Include(x => x.Payments)
                                      .ThenInclude(x => x.Cashier)
                                      .Include(x => x.ReturnedProducts)
                                      .ThenInclude(x => x.Product)
                                      .ThenInclude(x => x!.Item)
                                      .SingleOrDefaultAsync(x => x.Id == id);
		return order;
	}
    
    [HttpGet("byreceiptno")]
	public async Task<ActionResult<Order?>> GetOrderByReceiptNo(string? rno, Guid storeId)
	{
		if (_context.Orders == null || !_context.Orders.Any())
		{
			return NotFound();
		}
		var category = await _context.Orders.AsNoTracking()
                                      .AsSplitQuery()
                                      .Include(x => x.User)                                      
                                      .Include(x => x.Customer)
                                      .Include(x => x.ProductOrders)
                                      .Include(x => x.User)
                                      .Include(x => x.Store)
                                      .Include(x => x.Payments)
                                      .ThenInclude(x => x.Cashier)
                                      .FirstOrDefaultAsync(x => x.ReceiptNo == rno && x.StoreId == storeId);
		return category;
	}

	// PUT: api/Orders/5
	// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
	[HttpPut("{id}")]
	public async Task<IActionResult> PutOrder(Guid id, Order order)
	{
		if (id != order.Id)
		{
			return BadRequest();
		}

		_context.Entry(order).State = EntityState.Modified;
        foreach (var item in order.ProductOrders)
            _context.Entry(item).State = EntityState.Modified;
		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!OrderExists(id))
			{
				return NotFound();
			}
			else
			{
				throw;
			}
		}

		return NoContent();
	}
    
    [HttpGet("receiptno/{id}")]
    public async Task<ActionResult<int>> GetReceiptNo(Guid id)
    {
        var result = await _context.Orders.Where(b => b.StoreId == id)
                                          .Select(i => i.ReceiptNo)
                                          .CountAsync();
        result += 1;

        return result;
    }

    [HttpPost("receipt")]
    public async Task<IActionResult> GetReceipt(ReportData reportData)
    {
        var document = new ReceiptTemplate(reportData);
        var bytes = await document.Create();
        return File(bytes, "application/pdf");
    }

    [HttpGet("receipt/{id}")]
    public async Task<IActionResult> GetReceipt(Guid id)
    {
        var reportData = await _context.Orders.AsNoTracking().AsSplitQuery()
                                              .Include(x => x.ProductOrders)                                              
                                              .Include(x => x.Customer)
                                              .Include(x => x.Store)
                                              .Include(x => x.Payments)
                                              .Include(x => x.User)
                                              .ThenInclude(x => x.UserCredential)
                                              .Where(x => x.Id == id)
                                              .Select(x => new ReportData
                                              {
                                                Branch = x.Store,
                                                Customer = x.Customer,
                                                Order = x,
                                                Cashier = x.User.UserCredential.Username,
                                              }).FirstOrDefaultAsync();
        var document = new ReceiptTemplate(reportData);
        var bytes = await document.Create();
        return File(bytes, "application/pdf");
    }
    
    [HttpGet("validateorderid/{id}")]
    public async Task<ActionResult<bool>> ValidateOrderID(Guid id)
    {
        var result = await _context.Orders.AnyAsync(x => x.Id == id);
        return result;
    }
    
    [HttpGet("validatereceiptno/{id}")]
    public async Task<ActionResult<bool>> ValidateReceipt(string id)
    {
        var result = await _context.Orders.AnyAsync(x => x.ReceiptNo == id);
        return result;
    }

    // POST: api/Orders
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
	public async Task<ActionResult<Order>> PostOrder(Order category)
	{
		if (_context.Orders == null)
		{
			return Problem("Entity set 'AppDbContext.Orders'  is null.");
		}
		_context.Orders.Add(category);
		await _context.SaveChangesAsync();

		return CreatedAtAction("GetOrder", new { id = category.Id }, category);
	}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.Orders.FindAsync(id);
        if (record is null)
            return NotFound();

        var details = await _context.OrderItems.Where(x => x.OrderId == id).ToArrayAsync();
        foreach (var item in details)
        {
           var product = await _context.Products.FindAsync(item.ProductId);
           if (product is null)
            continue;
           product!.Dispensary.Where(x => x.id == item.StockId).FirstOrDefault()!.Quantity += item.Quantity;
           await _context.Products.Where(x => x.Id == product.Id).ExecuteUpdateAsync(x => x.SetProperty(p => p.Dispensary, product.Dispensary));
        }
        _context.Orders.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    public static GridDataResponse<OrderWithData> Paginate(IQueryable<Order> source, PaginationParameter parameters)
	{
		int totalItems = source.Count();
		int totalPages = (int)Math.Ceiling((double)totalItems / parameters.PageSize);

		List<OrderWithData> items = new();
		items = source
					.OrderByDescending(c => c.CreatedDate)
					.Skip(parameters.Page)
					.Take(parameters.PageSize)
                    .Select(x => new OrderWithData
                    {
                        Id = x.Id,
                        Date = x.OrderDate,
                        StoreName = x.Store!.BranchName,
                        OrderType = "Store",
                        ReceiptNo = x.ReceiptNo,
                        TotalAmount = x.TotalAmount,
                        CreatedDate = x.CreatedDate,
                        ModifiedDate = x.ModifiedDate
                    }).ToList();

		return new GridDataResponse<OrderWithData>
		{
			Data = items,
			TotalCount = totalItems
		};
	}

    [HttpPost("salesreport")]
    public async Task<ActionResult<List<SalesReportData>>> GetSalesReport(ReportFilter? filter)
    {
        if (filter == null)
            return BadRequest("Filter not set");

        List<SalesReportData> sales = new();
        if (filter.Type == "Store")
        {
            if (filter.Criteria == "Date")
                sales = _context.Orders.AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(x => x.User)            
                                            .Include(x => x.Customer)
                                            .Include(x => x.Store)
                                            .Include(x => x.ProductOrders)                       
                                            .Include(x => x.Payments)
                                            .ThenInclude(x => x.Cashier)
                                            .Where(x => x.StoreId == filter.StoreID && x.OrderDate == DateOnly.FromDateTime(filter.StartDate!.Value))
                                            .AsParallel()
                                            .Select(x => new SalesReportData
                                            {
                                                Id = x.Id,
                                                Date = x.OrderDate,
                                                StoreName = x.Store!.BranchName,
                                                CustomerName = x.Customer!.CustomerName,
                                                OrderType = "Store",
                                                ReceiptNo = x.ReceiptNo,
                                                TotalAmount = x.TotalAmount,
                                                SubTotal = x.SubTotal,
                                                Profit = AppState.CalculateProfit(x.ProductOrders) -x.ReturnedProducts.Sum(s => s.Cost.GetValueOrDefault()),
                                                PaymentStatus = x.GetPaymentStatus().ToString(),
                                                DeliveryStatus = x.GetDeliveryStatus(),
                                                HasDelievery = x.HasDelievery,
                                                Dispatched = x.Dispatched,
                                                Delivered = x.Delivered,
                                                OrderStatus = x.Status,
                                                Balance = x.Balance,
                                                Discount = x.Discount,

                                                Sale = x.User!.ToString(),
                                                Cashier = x.Payments.AsEnumerable().LastOrDefault()!.Cashier!.ToString(),
                                                CreatedDate = x.CreatedDate,
                                                ModifiedDate = x.ModifiedDate
                                            }).ToList();                                             
        }

        return sales;
    }


    [HttpPost("summaryreport")]
    public async Task<ActionResult<SalesReportTemplate>> SummaryReport(ReportFilter filter, CancellationToken cancellationToken)
    {
        List<SaleItem> items = [];
        var store = _context.Stores.AsParallel().FirstOrDefault(x => x.Id == filter.StoreID);
        if (store is null)
            return new SalesReportTemplate();

        List<Order> Orders = [];
        Orders = _context.Orders.AsNoTracking()
                                .AsSplitQuery()                                
                                .Include(x => x.ProductOrders)
                                .ThenInclude(x => x.ProductData)
                                .Include(x => x.ReturnedProducts.Where(r => r.Date.Date == filter!.StartDate.GetValueOrDefault().Date))
                                .Include(x => x.Payments)
                                .AsParallel()
                                .AsEnumerable()
                                .Where(x =>  x.StoreId == filter.StoreID && x.OrderDate == DateOnly.FromDateTime(filter!.StartDate!.Value))
                                .OrderByDescending(x => x.ModifiedDate)
                                .ToList();                    
        if (!Orders.Any())
            return new SalesReportTemplate
            {
                StoreName = store!.BranchName,
                BranchAddress = store!.BranchAddress,
                Criteria = filter.Criteria,
                StartDate = filter.StartDate!.Value.ToString("dd/MM/yyyy")
            };

        
        var Report = new SalesReportTemplate
        {
            StoreName = store!.BranchName,
            BranchAddress = store!.BranchAddress,
            Criteria = filter.Criteria,
            StartDate = filter.StartDate!.Value.ToString("dd/MM/yyyy")            
        };
         

        foreach (var x in Orders.AsParallel())
        {
            var saleItem = new SaleItem
            {
                Id = x.Id,
                Date = x.OrderDate,
                Option = "Store",                
                ReceiptNo = x.ReceiptNo,
                AmountPaid = x.Payments.Sum(p => p.Amount),
                TotalAmount = x.TotalAmount,
                SubTotal = x.SubTotal,
                Balance = x.Balance,
                BuyPrice = x.ProductOrders.Sum(x => x.BuyPrice) + x.ThirdPartyItems.Sum(x => x.Cost),
                SellPrice = x.ProductOrders.Sum(x => x.Cost) + x.ThirdPartyItems.Sum(x => x.Total),
                Profit = AppState.CalculateProfit(x.ProductOrders) + AppState.CalculateProfit(x.ThirdPartyItems) - x.ReturnedProducts.Sum(s => s.Cost.GetValueOrDefault()),            
                Quantity = x.ProductOrders.Sum(p => p.Quantity) + (int)x.ThirdPartyItems.Sum(x => x.Quantity),
                HasReturns = x.ReturnedProducts.Any(p => p.Quantity > 0),
                ReturnsQty = x.ReturnedProducts.Sum(p => p.Quantity.GetValueOrDefault()),
                HasOrderItems = x.ProductOrders.Any() || x.ThirdPartyItems.Any(),
                Refund = x.ReturnedProducts.Sum(r => (decimal)r.Quantity! * r.Cost.GetValueOrDefault()),                
                Discount = x.Discount,
            };

            //saleItem.Cashier = x.Payments.AsEnumerable().LastOrDefault()!.Cashier!.ToString();
            Report.SaleItems.Add(saleItem);
        }
        return Report;
    }

	private bool OrderExists(Guid id)
	{
		return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
	}
}
