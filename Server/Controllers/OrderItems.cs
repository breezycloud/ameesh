using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Models.Orders;
using Shared.Models.Products;
using Shared.Models.Reports;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderItems : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderItems> _logger;

    public OrderItems(AppDbContext context, ILogger<OrderItems> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductOrderItem>>> GetItems() => await _context.OrderItems.ToArrayAsync();


    [HttpGet("stockoutreport/{id}")]
    public async Task<ActionResult<UserSoldProduct>> GetStockOut(Guid id)
    {
        var products = _context.OrderItems.AsNoTracking().AsSplitQuery().Include(x => x.Order).Include(x => x.ProductData).Where(x => x.Order!.UserId == id).AsEnumerable().AsParallel().GroupBy(x => x.ProductId).Select(x => new SoldProducts
        {
            Id = x.Key,
            ProductName = x.FirstOrDefault()!.Product,
            QtySold = x.Sum(x => x.Quantity),
            DispensaryQty = x.Select(p => p.ProductData).Sum(p => p!.DispensaryQuantity)
        }).ToList();

        var User = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (!products.Any())
            return new UserSoldProduct
            {
                ReportDate = DateTime.Now,
                User = User!.ToString(),
                Products = []
            };

        return new UserSoldProduct
        {
            ReportDate = DateTime.Now,
            User = User!.ToString(),
            Products = products
        };
    }

    // PUT: api/OrdersItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutOrder(Guid id, ProductOrderItem item)
    {
        if (id != item.Id)
        {
            return BadRequest();
        }

        _context.Entry(item).State = EntityState.Modified;        

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!OrderItemExists(id))
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
    
    // PUT: api/OrdersItems/BulkUpdate/[]
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("bulkupdate")]
    public async Task<IActionResult> PutOrderItems(ProductOrderItem[] items)
    {
        try
        {
            await _context.BulkUpdateAsync(items);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }

        return NoContent();
    }

    private bool OrderItemExists(Guid id)
    {
        return (_context.OrderItems?.Any(e => e.Id == id)).GetValueOrDefault();
    }


}
