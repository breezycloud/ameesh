using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Server.Services;
using Shared.Helpers;
using Shared.Models.Orders;
using Shared.Models.Products;
using Shared.Models.Reports;
using Shared.Models.Expenses;
using Shared.Enums;

namespace Server.Hubs;

public class SignalRHubs : Hub
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SignalRHubs> _logger;

    public SignalRHubs(IServiceProvider service, ILogger<SignalRHubs> logger)
    {
        _services = service;
        _logger = logger;
    }
    private bool IsExecuting = false;
    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Connected: {0}", Context.ConnectionId);
        //_ = CancelOrders();
        return base.OnConnectedAsync();
    }
    public async Task RefreshWelfare() => await Clients.All.SendAsync("RefreshWelfare");
    public async Task UpdateUsers() => await Clients.All.SendAsync("UpdateUsers");
    public async Task PrintBillSlip(Guid UserID) => await Clients.All.SendAsync("PrintBillSlip", UserID);
    public async Task PrintBill(Guid UserID, string Option, byte[] buffer)
        => await Clients.All.SendAsync("PrintBill", UserID, Option, buffer);
    public async Task UpdateExpenses() => await Clients.All.SendAsync("UpdateExpenses");
    public async Task PrintOrder(Guid StoreID, ReportData? report) => await Clients.All.SendAsync("PrintOrder", StoreID, report);
    public async Task RefreshOrderTimer() => await Clients.All.SendAsync("RefreshOrderTimer");
    public async Task DirectPrint(Guid storeID, byte[] buffer) => await Clients.All.SendAsync("DirectPrint", storeID, buffer);
    public async Task UpdateStores() => await Clients.All.SendAsync("UpdateStores");
    public async Task UpdateCustomers() => await Clients.All.SendAsync("UpdateCustomers");
    public async Task UpdateReferrers() => await Clients.All.SendAsync("UpdateReferrers");
    public async Task UpdateDealers() => await Clients.All.SendAsync("UpdateDealers");
    public async Task UpdateCategories() => await Clients.All.SendAsync("UpdateCategories");
    public async Task UpdateProducts() => await Clients.All.SendAsync("UpdateProducts");
    public async Task UpdateProduct() => await Clients.All.SendAsync("UpdateProduct");
    public async Task UpdateItems() => await Clients.All.SendAsync("UpdateItems");
    public async Task UpdateItem() => await Clients.All.SendAsync("UpdateItem");
    public async Task UpdateServices() => await Clients.All.SendAsync("UpdateServices");
    public async Task UpdateCharges() => await Clients.All.SendAsync("UpdateCharges");
    public async Task UpdateBillings() => await Clients.All.SendAsync("UpdateBillings");
    public async Task UpdateLabOrders() => await Clients.All.SendAsync("UpdateLabOrders");
    public async Task UpdateLabOrder() => await Clients.All.SendAsync("UpdateLabOrder");
    public async Task UpdateOrders() => await Clients.All.SendAsync("UpdateOrders");
    public async Task UpdateOrder() => await Clients.All.SendAsync("UpdateOrder");
    public async Task UpdateProductQuatity(Guid id, ProductOrderItem[] items)
    {
        _logger.LogInformation("Background update in progress...");
        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var service = scope.ServiceProvider.GetRequiredService<OrderService>();
        await service.UpdateProductQuantity(id, items);
        await UpdateProducts();
    }

    public async Task RemoveProductQuantity(Guid id, List<OrderCartRow> items)
    {
        _logger.LogInformation("Background update in progress...");
        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var service = scope.ServiceProvider.GetRequiredService<OrderService>();
        await service.RemoveProductQuantity(id, items);
        await UpdateProducts();
    }

    public async Task AddProductQuantity(Guid id, List<OrderCartRow> items)
    {
        _logger.LogInformation("Background update in progress...");
        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var service = scope.ServiceProvider.GetRequiredService<OrderService>();
        await service.AddProductQuantity(id, items);
        await UpdateProducts();
    }

    public async Task ReturnSuspendedBills(List<SuspendBills> items)
    {
        _logger.LogInformation("Suspended Bills job on progress...");        
        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var service = scope.ServiceProvider.GetRequiredService<OrderService>();
        await service.AddProductQuantity(items);
        await UpdateProducts();
    }
    
    public async Task UpdateStock(Guid id, string Option, ProductOrderItem[] items)
    {
        _logger.LogInformation("Background update in progress...");
        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var service = scope.ServiceProvider.GetRequiredService<OrderService>();
        await service.UpdateStock(id, Option, items);
        await UpdateProducts();
        await UpdateOrders();
        await UpdateOrder();
    }

    public async Task AutoExpense(ExpenseEntryDto expense)
    {
        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eType = await db.ExpenseTypes.FindAsync(expense.TypeId);
        if (eType is null)
        {
            _logger.LogInformation("Expense type is not found");
            return;
        }
        _logger.LogInformation("Adding Automatic expense");
        await db.Expenses.AddAsync(new Expense 
        {
            Id = Guid.NewGuid(),
            UserId = expense.UserId,
            Date = expense.CreatedDate,
            StoreId = expense.StoreId,
            Description = "Automatic order expense",
            TypeId = expense.TypeId,
            Reference = expense.ReferenceNo,
            PaymentMode = PaymentMode.None,
            Amount = 4000,
            CreatedDate = expense.CreatedDate
        });
        await db.SaveChangesAsync();
        _logger.LogInformation("Saved Automatic expense");

    }

    public async Task AllExpiryProducts(Guid id)
    {
        _logger.LogInformation("Background update in progress...");
        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var service = scope.ServiceProvider.GetRequiredService<OrderService>();
        int total = await service.GetTotalExpiryProducts(id);
    }

    public async Task CancelOrders()
    {
        if (IsExecuting) return;
        IsExecuting = true;
        var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        using var service = scope.ServiceProvider.GetRequiredService<OrderService>();
        CancellationToken token = new CancellationToken();
        await service.CancelOrder(token);
        await RefreshOrderTimer();
        IsExecuting = false;
    }
    
    public async Task SyncProgress(SyncProgress progress)
    {
        await Clients.All.SendAsync("SyncProgress", progress);
    }
}
