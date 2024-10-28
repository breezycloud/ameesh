using Microsoft.EntityFrameworkCore;
using Server.Context;

namespace Server.Config;


public static class SyncConfig
{    
    public static void SyncSetup(IServiceProvider services)
    {
        var factory = services.GetRequiredService<IServiceScopeFactory>();
        using var scope = factory.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();                
    }

    public static IServiceCollection AddSyncConfig(this IServiceCollection services)
    {                 
        return services;
    }
}