using Microsoft.EntityFrameworkCore;
using Shared.Models.Company;
using Shared.Models.Customers;
using Shared.Models.Orders;
using Shared.Models.Products;
using Shared.Models.Users;
using Shared.Models.Expenses;
using Shared.Models.Logging;
using Shared.Models.Welfare;

namespace Server.Context;

public partial class ServerDbContext : DbContext
{

    public ServerDbContext(DbContextOptions<ServerDbContext> options)
        : base(options)
    {
        
    }
    public DbSet<Store> Stores { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserCredential> UserCredentials { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Product> Products { get; set; }    
    public DbSet<Order> Orders { get; set; }        
    public DbSet<ProductOrderItem> OrderItems { get; set; }        
    public DbSet<Item> Items { get; set; }
    public DbSet<ReturnedProduct> ReturnedProducts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public virtual DbSet<LogMessage> Logs { get; set; }
    public virtual DbSet<RequestAudit> Audits { get; set; }
    public DbSet<Expense> Expenses { get; set; } = default!;
    public DbSet<ExpenseType> ExpenseTypes { get; set; } = default!;
    public DbSet<Salary> Salaries { get; set; } = default!;
    public DbSet<SalaryAdvance> SalaryAdvances { get; set; } = default!;
    public DbSet<SalaryBonus> SalaryBonus { get; set; } = default!;
    public DbSet<Penalty> Penalties { get; set; } = default!;
    public DbSet<OrderReferer> Referrals { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}