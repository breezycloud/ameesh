using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;

using Shared.Models.Orders;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(AppDbContext _context) : ControllerBase
{

    [HttpGet("order/{id}")]
    public async Task<ActionResult<List<Payment>>> GetOrderPayments(Guid id)
    {
        return await _context.Payments.Where(x => x.OrderId == id).ToListAsync();
    }

    // PUT: api/Labpayment/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPayment(Guid id, Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest();
        }

        _context.Entry(payment).State = PaymentExists(id) ? EntityState.Modified : EntityState.Added;
        var payments = _context.Payments.AsParallel().Where(x => x.Amount < 0).ToList();
        foreach (var item in payments)
        {
            _context.Payments.Remove(item);            
        }
        
        try
        {
            await _context.SaveChangesAsync();
            var order = await _context.Orders.Include(x => x.Payments).FirstOrDefaultAsync(x => x.Id == payment.OrderId);
            Console.WriteLine("Balance: {0}",order!.Balance.ToString("N2"));
            if (order!.Balance < 0)
            {
                order!.Status = Shared.Enums.OrderStatus.Completed;
            }        
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PaymentExists(id))
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

    [HttpPut("order")]
    public async Task<IActionResult> PutPayments(Payment[] pays)
    {
        Guid OrderId = Guid.Empty;
        foreach (var payment in pays)
        {
            OrderId = payment.OrderId;
            payment.Cashier = null;
            _context.Entry(payment).State = PaymentExists(payment.Id) ? EntityState.Modified : EntityState.Added;
        }
        
        var payments = _context.Payments.AsParallel().Where(x => x.OrderId == OrderId && x.Amount < 0).ToList();
        foreach (var item in payments)
        {
            _context.Payments.Remove(item);            
        }
        
        try
        {
            await _context.SaveChangesAsync();
            var order = await _context.Orders.Include(x => x.Payments).FirstOrDefaultAsync(x => x.Id == OrderId);
            if (order!.Balance < 0)
            {
                order!.Status = Shared.Enums.OrderStatus.Completed;
            }        
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.Payments.FindAsync(id);
        if (record is null)
            return NotFound();

        _context.Payments.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool PaymentExists(Guid id)
    {
        return (_context.Payments?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
