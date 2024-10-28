using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Server.Context;
using Shared.Models.Logging;

namespace Server.Services;

public class AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger, IDbContextFactory<AppDbContext> context)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<AuditMiddleware> _logger = logger;
    private readonly IDbContextFactory<AppDbContext> _context = context;
    public async Task Invoke(HttpContext context)
    {
        string method = context.Request.Method;
        if (context.Request.Method is "PUT" or "POST" or "DELETE")
        {
            // Capture request details
            var audit = new RequestAudit
            {
                User = context.User?.Identity?.Name,
                Method = method,
                Path = context.Request.Path,
                CreatedAt = DateTime.UtcNow,
            };

            await _next(context);

            // After processing, get the response status code
            int statusCode = context.Response.StatusCode;

            // Log details including status code using your logging library        
            //using var db = _context.CreateDbContext();
            //db.Audits.Add(audit);
            //db.SaveChanges();
            _logger.LogInformation($"Request: {audit.Path} - {audit.User} ({audit.Method}) - {audit.CreatedAt} - Status Code: {statusCode}");
        }        
    }
}
