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
        using var transaction = await _context.Database.BeginTransactionAsync();
        Guid OrderId = Guid.Empty;
        foreach (var payment in pays)
        {
            OrderId = payment.OrderId;
            payment.Cashier = null;
            _context.Entry(payment).State = PaymentExists(payment.Id) ? EntityState.Modified : EntityState.Added;
        }

        var currentPaymentSum = await _context.Payments.Where(x => x.OrderId == OrderId).SumAsync(x => x.Amount);
        var Order = await _context.Orders.Include(x => x.Payments).Include(x => x.ProductOrders).FirstOrDefaultAsync(x => x.Id == OrderId);
        if (currentPaymentSum > Order!.Balance)
        {
             return BadRequest($"Payment amount {currentPaymentSum} exceeds remaining balance {Order!.Balance}.");
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
            await transaction.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }

        return NoContent();
    }

    // public async Task MakePayment()
    // {
    //     using var transaction = await _context.Database.BeginTransactionAsync(ct);
    //     try
    //     {
    //         // 1. Check for duplicate idempotency key
    //         if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
    //         {
    //             var existingPayment = await _context.Payments
    //                 .FirstOrDefaultAsync(p => p.IdempotencyKey == request.IdempotencyKey, ct);

    //             if (existingPayment != null)
    //             {
    //                 // Return existing payment — idempotent!
    //                 return Result.Success(existingPayment);
    //             }
    //         }

    //         // 2. Load order with tracking + include payments for accurate balance
    //         var order = await _context.PharmacyOrders
    //             .Include(o => o.Payments)
    //             .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

    //         if (order == null)
    //             return Result.Failure<Payment>("Order not found");

    //         // 3. Calculate current balance (SubTotal - Payments)
    //         var currentBalance = order.SubTotal - order.Payments.Sum(p => p.Amount);

    //         // 4. Validate payment doesn't exceed balance
    //         if (request.Amount > currentBalance)
    //         {
    //             return Result.Failure<Payment>($"Payment amount {request.Amount} exceeds balance {currentBalance}");
    //         }

    //         // 5. Create new payment
    //         var payment = new Payment
    //         {
    //             Id = Guid.NewGuid(),
    //             PharmacyOrderId = request.OrderId,
    //             Amount = request.Amount,
    //             PaymentMode = request.PaymentMode,
    //             PaymentDate = DateTime.UtcNow,
    //             CreatedDate = DateTime.UtcNow,
    //             IdempotencyKey = request.IdempotencyKey, // Store it
    //             UserId = /* get from auth/user context */
    //         };

    //         _context.Payments.Add(payment);
    //         order.ModifiedDate = DateTime.UtcNow; // optional

    //         // 6. Save & commit
    //         await _context.SaveChangesAsync(ct);
    //         await transaction.CommitAsync(ct);

    //         return Result.Success(payment);
    //     }
    //     catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE constraint") == true)
    //     {
    //         // Idempotency key collision — fetch and return existing
    //         var existing = await _context.Payments
    //             .FirstOrDefaultAsync(p => p.IdempotencyKey == request.IdempotencyKey, ct);
    //         return Result.Success(existing!);
    //     }
    //     catch (Exception ex)
    //     {
    //         await transaction.RollbackAsync(ct);
    //         _logger.LogError(ex, "Payment processing failed for order {OrderId}", request.OrderId);
    //         return Result.Failure<Payment>("Payment processing failed");
    //     }
        
    // }

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
