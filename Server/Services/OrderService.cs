using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Models.Orders;
using Shared.Models.Products;

namespace Server.Services;

public interface IOrderService
{
    public Task CancelOrder(CancellationToken token);
    public Task RemovePaymentDiscrepancies(CancellationToken token);
    public Task UpdateProductQuantity(Guid id, ProductOrderItem[] items);
    public Task<int> GetTotalExpiryProducts(Guid id);
    public Task<int> GetTotalDispensaryExpiryProducts(Guid id);
    public Task<int> GetTotalStoreExpiryProducts(Guid id);
    public Task RemoveProductQuantity(Guid id, List<OrderCartRow> items);
    public Task AddProductQuantity(Guid id, List<OrderCartRow> items);
    Task AddProductQuantity(List<SuspendBills> bills);
}
public class OrderService(ILogger<OrderService> _logger, AppDbContext _context) : IOrderService, IDisposable
{    
    public async Task UpdateProductQuantity(Guid id, ProductOrderItem[] items)
    {
        foreach (var item in items)
        {
            _logger.LogInformation("Updating product quantity");
            var product = await _context.Products.Where(x => x.StoreId == id && x.Id == item.ProductId).FirstOrDefaultAsync();
            product!.Dispensary.FirstOrDefault(x => x.id == item.StockId)!.Quantity -= item.Quantity;
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully Updated {0} Quantity", item.Product);
        }
    }

    public async Task RemoveProductQuantity(Guid id, List<OrderCartRow> items)
    {
        foreach (var item in items)
        {
            _logger.LogInformation("Updating product quantity");
            var product = await _context.Products.Where(x => x.StoreId == id && x.Id == item.Product!.Id).FirstOrDefaultAsync();
            product!.Dispensary.FirstOrDefault(x => x.id == item.Stock!.id)!.Quantity -= item.Quantity;
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully Updated {0} Quantity", item.Product!.ProductName);
        }
    }

    public async Task AddProductQuantity(Guid id, List<OrderCartRow> items)
    {
        foreach (var item in items)
        {
            _logger.LogInformation("Updating product quantity");
            var product = await _context.Products.Where(x => x.StoreId == id && x.Id == item.Product!.Id).FirstOrDefaultAsync();
            product!.Dispensary.FirstOrDefault(x => x.id == item.Stock!.id)!.Quantity += item.Quantity;
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully Updated {0} Quantity", item.Product!.ProductName);
        }
    }

    public async Task AddProductQuantity(List<SuspendBills> bills)
    {
        foreach (var bill in bills)
        {
            foreach (var item in bill.Rows!)
            {
                _logger.LogInformation("Updating product quantity");
                var product = await _context.Products.Where(x => x.Id == item.Product!.Id).FirstOrDefaultAsync();
                product!.Dispensary.FirstOrDefault(x => x.id == item.Stock!.id)!.Quantity += item.Quantity;
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully Updated {0} Quantity", item.Product!.ProductName);
            }
        }        
    }
    public async Task RemovePaymentDiscrepancies(CancellationToken token)
    {
        _logger.LogInformation("Checking for payment discrepancies");
        var total = _context.Orders.Include(x => x.Payments).Where(x => x.PaymentConfirmed && !x.Payments.Any()).Count();
        _logger.LogInformation("payment issues {0}", total);

        // var discrepancies = _context.Orders.Include(x => x.Payments).AsSplitQuery().AsEnumerable().Where(x => x.Balance < 0);
        // if (!discrepancies.Any())
        // {
        //     _logger.LogInformation("no payment discrepancy found");
        //     return;
        // }

        // _logger.LogInformation("Payment discrepancy found");
        // foreach (var order in discrepancies)
        // {
        //     _logger.LogInformation("Resolving payment discrepancy for order {0}", order.ReceiptNo);
        //     var lastPayment = order.Payments.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
        //     if (lastPayment is null)
        //         continue;

        //     if (lastPayment.Amount > 0)
        //     {
        //         await _context.Payments.Where(x => x.Id == lastPayment.Id).ExecuteDeleteAsync(token);
        //     }
        // }
        _logger.LogInformation("Payment issue(s) resolved");
        
    }

    public async Task UpdateStock(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking for negative stock");
        var stores = await _context.Stores.ToListAsync(cancellationToken);
        foreach (var store in stores)
        {
            _logger.LogInformation($"Checking for negative stock {store.BranchName}");
            var products = _context.Products.Include(x => x.Item).AsEnumerable().Where(x => x.StoreId == store.Id && x.Dispensary.Any(d => d.Quantity < 0) || x.Stocks.Any(s => s.Quantity < 0)).ToList();
            if (!products.Any())
            {
                _logger.LogInformation("No negative stock found");
                return;
            }
            foreach (var product in products)
            {
                foreach (var stock in product.Dispensary.AsEnumerable().Where(x => x.Quantity < 0))
                {
                    _logger.LogInformation("Removing negative stock for {0} {1}", "Dispensary", product.Item?.ProductName);
                    stock.Quantity = 0;
                }
                foreach (var stock in product.Stocks.AsEnumerable().Where(x => x.Quantity < 0))
                {
                    _logger.LogInformation("Removing negative stock for {0} {1}", "Stocks", product.Item?.ProductName);
                    stock.Quantity = 0;
                }
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }
        
    }
    public async Task UpdateStock(Guid id, string Option, ProductOrderItem[] items)
    {
        foreach (var item in items)
        {
            _logger.LogInformation("Updating product quantity");
            var product = await _context.Products.Where(x => x.Id == item.ProductId).FirstOrDefaultAsync();
            if (Option == "Dispensary")
                product!.Dispensary.FirstOrDefault(x => x.id == item.StockId)!.Quantity -= item.Quantity;
            else
                product!.Stocks.FirstOrDefault(x => x.id == item.StockId)!.Quantity -= item.Quantity;
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully Updated {0} Quantity", item.Product);
        }
    }
    public async Task CleanOverpaidOrders(CancellationToken ct)
    {                
        var problematicOrders = _context.Orders
            .Include(o => o.Payments)
            .Include(x => x.ProductOrders)
            .AsEnumerable()
            .Where(o => o.Payments.Sum(p => p.Amount) > o.SubTotal)
            .ToList();

        var removedPayments = new List<Payment>();
        var affectedOrderIds = new List<Guid>();

        // using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            foreach (var order in problematicOrders)
            {
                var payments = order.Payments.OrderBy(p => p.CreatedDate).ToList();
                decimal runningTotal = 0;

                foreach (var payment in payments)
                {
                    if (runningTotal + payment.Amount > order.SubTotal)
                    {
                        // This payment causes overpayment → mark for removal
                        _context.Payments.Remove(payment);
                        removedPayments.Add(payment);
                    }
                    runningTotal += payment.Amount;
                }

                if (removedPayments.Any(p => p.OrderId == order.Id))
                {
                    affectedOrderIds.Add(order.Id);
                    order.ModifiedDate = DateTime.UtcNow;
                }
            }

            if (removedPayments.Any())
            {
                await _context.SaveChangesAsync(ct);
                // await transaction.CommitAsync(ct);

                // Log what was removed
                Console.WriteLine(
                    "CLEANUP: Removed {0} duplicate/over payments from {1} orders: {2}",
                    removedPayments.Count,
                    affectedOrderIds.Distinct().Count(),
                    string.Join(", ", affectedOrderIds.Distinct()));

                // var removed = new
                // {
                //     RemovedPaymentCount = removedPayments.Count,
                //     AffectedOrders = affectedOrderIds.Distinct().ToList(),
                //     Details = removedPayments.Select(p => new
                //     {
                //         p.Id,
                //         p.Amount,
                //         p.CreatedDate,
                //         p.OrderId
                //     })
                // };

                // Console.WriteLine(removed.RemovedPaymentCount);
            }
            else
            {
                // await transaction.RollbackAsync(ct);
                Console.WriteLine("No overpayments found to remove.");
            }
        }
        catch (Exception ex)
        {
            // await transaction.RollbackAsync(ct);
            Console.WriteLine("Failed to cleanup overpaid orders {0}", ex);
            Console.WriteLine("Cleanup failed. Check logs.");
        }

    }

    public async Task<int> GetTotalExpiryProducts(Guid id)
    {
        _logger.LogInformation("Checking for expiry products");
        int dispensary = await GetTotalDispensaryExpiryProducts(id);
        int store = await GetTotalStoreExpiryProducts(id);
        return dispensary + store;
    }

    public async Task<int> GetTotalDispensaryExpiryProducts(Guid id)
    {
        _logger.LogInformation("Checking dispensary expiry products");
        int dispensary = await _context.Products.Where(x => x.StoreId == id).SelectMany(x => x.Dispensary).Where(x => x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).CountAsync();
        return dispensary;
    }
    
    public async Task<int> GetTotalStoreExpiryProducts(Guid id)
    {
        _logger.LogInformation("Checking store expiry products");
        int store = await _context.Products.Where(x => x.StoreId == id).SelectMany(x => x.Dispensary).Where(x => x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).CountAsync();
        return store;
    }
    public async Task CancelOrder(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _logger.LogInformation("Process is stopped");
            return;
        }
        _logger.LogInformation("Checking orders in progress...");
        var OrdersToCancel = await _context.Orders.AsSplitQuery().Include(c => c.Customer).Include(x => x.ProductOrders).Where(x => x.Status == Shared.Enums.OrderStatus.Pending && x.Customer!.HasDiscount == false).ToListAsync();
        if (!OrdersToCancel.Any())
        {
            _logger.LogInformation("No orders to cancel...");
            return;
        }
        var items = OrdersToCancel.Where(x => x.Balance > 0 && DateTime.UtcNow.Subtract(x.CreatedDate).Hours >= 1).Select(x => new
        {
            id = x.Id,
            storeId = x.StoreId,
            items = x.ProductOrders.Select(t => new { productId = t.ProductId, stockId = t.StockId, qty = t.Quantity }).ToList(),
            remainingTime = DateTime.UtcNow.Subtract(x.CreatedDate).Hours
        }).ToList();
        _logger.LogInformation("{0} orders found...", items.Count);
        foreach (var item in items)
        {
            foreach (var row in item.items)
            {
                var product = await _context.Products.Where(x => x.StoreId == item.storeId && x.Id == row.productId).FirstOrDefaultAsync();
                if (product is null)
                    continue;

                if (!product!.Dispensary.Any(x => x.id == row.stockId))
                    continue;
                product!.Dispensary.FirstOrDefault(x => x.id == row.stockId)!.Quantity += row.qty;
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await _context.OrderItems.Where(x => x.OrderId == item.id && x.ProductId == row.productId).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, Shared.Enums.OrderStatus.Canceled));
            }
            await _context.Orders.Where(x => x.Id == item.id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, Shared.Enums.OrderStatus.Canceled));
        }
    }

    public void Dispose()
    {
    }
}
