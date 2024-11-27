using Microsoft.EntityFrameworkCore;
using Shared.Models.Logging;

namespace Server.Context;

public partial class LoggingContext : DbContext
    {
        public LoggingContext()
        {
        }

        public LoggingContext(DbContextOptions<LoggingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<LogMessage> Logs { get; set; }
        public virtual DbSet<RequestAudit> Audits { get; set; }
        
    }