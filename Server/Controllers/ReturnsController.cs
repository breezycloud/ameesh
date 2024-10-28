using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Models.Products;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[ResponseCache(CacheProfileName = "Default60")]
public class ReturnsController(AppDbContext _context) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReturnedProduct>>> GetReturns() =>
        await _context.ReturnedProducts.OrderByDescending(x => x.Date).ToArrayAsync();

    // PUT: api/ReturnedProduct/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(Guid id, RefundPayload payload)
    {
        if (id != payload.Product!.Id)
        {
            return BadRequest();
        }
        _context.Entry(payload!.Product).State = ProductExists(id) ? EntityState.Modified : EntityState.Added;                    

        try
        {
            await _context.SaveChangesAsync();
            await _context.Orders.Where(x => x.Id == payload.Product.OrderId).ExecuteUpdateAsync(s => s.SetProperty(p => p.ModifiedDate, DateTime.Now));
            var order = await _context.Orders.FindAsync(payload!.Product!.OrderId);
            var customer = await _context.Customers.FindAsync(order!.CustomerId);
            if (payload.Product.RefundType == "Credit" && !customer!.Regular)
            {
                await _context.Customers.Where(x => x.Id == customer!.Id).ExecuteUpdateAsync(s => s.SetProperty(p => p.StoreCredit, p => p.StoreCredit + payload!.Product!.Cost).SetProperty(p => p.ModifiedDate, DateTime.Now));
            }
            if (!ProductExists(id))
            {
                await _context.OrderItems.Where(x => x.ProductId == payload.Product.ProductId)
                                     .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, f => f.Quantity + payload.Product.Quantity));
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
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

    // POST: api/ReturnedProduct
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

    [HttpPost]
    public async Task<ActionResult<ReturnedProduct>> PostProduct(ReturnedProduct product)
    {
        if (_context.ReturnedProducts == null)
        {
            return Problem("Entity set 'AppDbContext.ReturnedProducts'  is null.");
        }
        _context.ReturnedProducts.Add(product);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.ReturnedProducts.FindAsync(id);
        if (record is null)
            return NotFound();

        _context.ReturnedProducts.Remove(record);
        await _context.OrderItems.Where(x => x.ProductId == record.ProductId)
                                 .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, f => f.Quantity + record.Quantity));
        await _context.SaveChangesAsync();
        var order = await _context.Orders.FindAsync(record!.OrderId);
        var customer = await _context.Customers.FindAsync(order!.CustomerId);
        if (record.RefundType == "Credit" && !customer!.Regular)
        {
            await _context.Customers.Where(x => x.Id == customer.Id).ExecuteUpdateAsync(s => s.SetProperty(p => p.StoreCredit, p => p.StoreCredit - record!.Cost).SetProperty(p => p.ModifiedDate, DateTime.Now));
        }
        return NoContent();
    }

    private bool ProductExists(Guid id)
    {
        return (_context.ReturnedProducts?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
