using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Server.Hubs;
using Server.Services;
using Shared.Models.Products;

namespace Pharmacy.Server.BackgroundJob;

public class ServerPeriodicJob : BackgroundService, IDisposable
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ServerPeriodicJob> _logger;
    private readonly IHubContext<SignalRHubs> _context;
    private readonly PeriodicTimer _PrinterTimer = new(TimeSpan.FromSeconds(60));    
    private readonly PeriodicTimer _CancelOrderTimer = new(TimeSpan.FromMinutes(1));
    public ServerPeriodicJob(ILogger<ServerPeriodicJob> logger, IHubContext<SignalRHubs> context, IServiceProvider services)
    {
        _logger = logger;
        _context = context;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("My Background Service is starting.");        
        //while (await _PrinterTimer.WaitForNextTickAsync(stoppingToken))
        //{            
        //    _logger.LogInformation("Check expired orders");
        //}
        while (await _CancelOrderTimer.WaitForNextTickAsync(stoppingToken))
        {
            var scopeFactory = _services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            using var service = scope.ServiceProvider.GetRequiredService<OrderService>();            
            await service.RemovePaymentDiscrepancies(stoppingToken);
        }
    }
    void IDisposable.Dispose()
    {
        _PrinterTimer.Dispose();
        _CancelOrderTimer.Dispose();
        _logger.LogInformation("timers disposed");
    }
}
