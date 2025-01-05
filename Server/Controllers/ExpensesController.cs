using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Extensions;
using QuestPDF.Fluent;
using Server.Context;
using Server.Pages.Reports.Templates.Welfare;
using Shared.Helpers;
using Shared.Models.Expenses;
using Shared.Models.Orders;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExpensesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<GridDataResponse<Expense>>> GetPagedExpenses(PaginationParameter parameter)
    {
        GridDataResponse<Expense> response = new();
        if (parameter.FilterId is not null)
        {
            response.Data = await _context.Expenses.AsNoTracking()
                                                   .AsSplitQuery()
                                                   .Include(t => t.ExpenseType)
                                                   .Where(x => x.StoreId == parameter.FilterId.GetValueOrDefault())
                                                   .OrderByDescending(x => x.CreatedDate)
                                                   .Skip(parameter.Page)
                                                   .Take(parameter.PageSize)
                                                   .ToListAsync();
            response.TotalCount = await _context.Expenses.AsNoTracking().Where(x => x.StoreId == parameter.FilterId.GetValueOrDefault()).CountAsync();
        }
        else
        {
            response.Data = await _context.Expenses.AsNoTracking()
                                                   .AsSplitQuery()
                                                   .Include(t => t.ExpenseType)
                                                   .OrderByDescending(x => x.CreatedDate)
                                                   .Skip(parameter.Page)
                                                   .Take(parameter.PageSize)
                                                   .ToListAsync();

            response.TotalCount = await _context.Expenses.AsNoTracking()
                                                         .CountAsync();
        }

        return Ok(response);
    }

    [HttpPost("report")]
    public async Task<ActionResult> Report(ExpenseReportFilter criteria, CancellationToken cancellationToken)
    {                
        var query = _context.Expenses.AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.User)
            .Include(x => x.ExpenseType)
            .AsQueryable();

        // Apply ID filter at database level
        if (criteria.id != null)
        {
            query = query.Where(x => x.TypeId == criteria.id.Value);
        }

        // Apply date filters at database level
        if (criteria.type == "Range")
        {
            query = query.Where(x => x.CreatedDate.Date >= criteria.from.Value.Date 
                && x.CreatedDate.Date <= criteria.to!.Value.Date);
        }
        else
        {
            query = query.Where(x => x.CreatedDate.Date == criteria.from.Value.Date);
        }

        // Execute the query once with the projection
        var expenses = await query
            .Select(x => new ExpenseData(
                x.TypeId,
                x.ExpenseType!.Expense,
                x.Description,
                x.Reference,
                x.User!.ToString(),
                x.Amount!.Value,
                x.PaymentMode,
                x.CreatedDate))
            .ToArrayAsync();

        var pdf = new ExpenseReport(expenses).GeneratePdf();        
        return File(pdf, "application/pdf");
    }

    // GET: api/Expenses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpense()
    {
        return await _context.Expenses.ToListAsync();
    }

    // GET: api/Expenses/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(Guid id)
    {
        var expense = await _context.Expenses.FindAsync(id);

        if (expense == null)
        {
            return NotFound();
        }

        return expense;
    }

    // PUT: api/Expenses/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutExpense(Guid id, Expense expense)
    {
        if (id != expense.Id)
        {
            return BadRequest();
        }

        _context.Entry(expense).State = ExpenseExists(id) ? EntityState.Modified : EntityState.Added;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ExpenseExists(id))
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

    // POST: api/Expenses
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Expense>> PostExpense(Expense expense)
    {
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetExpense", new { id = expense.Id }, expense);
    }

    // DELETE: api/Expenses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null)
        {
            return NotFound();
        }

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ExpenseExists(Guid id)
    {
        return _context.Expenses.Any(e => e.Id == id);
    }
}
