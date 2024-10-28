using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Helpers;
using Shared.Models.Expenses;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpenseTypesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("paged")]
        public async Task<ActionResult<GridDataResponse<ExpenseType>>> GetPagedExpenses(PaginationParameter parameter)
        {
            GridDataResponse<ExpenseType> response = new();
            response.Data = await _context.ExpenseTypes.AsNoTracking()
                                                       .AsSplitQuery()
                                                       .Include(x => x.Expenses)
                                                       .OrderByDescending(x => x.CreatedDate)
                                                       .Skip(parameter.Page)
                                                       .Take(parameter.PageSize)
                                                       .ToListAsync();

            response.TotalCount = await _context.ExpenseTypes.AsNoTracking()
                                                         .CountAsync();
            return Ok(response);
        }

        // GET: api/ExpenseTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseType>>> GetExpenseType()
        {
            return await _context.ExpenseTypes.ToListAsync();
        }

        // GET: api/ExpenseTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseType>> GetExpenseType(Guid id)
        {
            var expenseType = await _context.ExpenseTypes.FindAsync(id);

            if (expenseType == null)
            {
                return NotFound();
            }

            return expenseType;
        }

        // PUT: api/ExpenseTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpenseType(Guid id, ExpenseType expenseType)
        {
            if (id != expenseType.Id)
            {
                return BadRequest();
            }

            _context.Entry(expenseType).State = ExpenseTypeExists(id) ? EntityState.Modified : EntityState.Added;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseTypeExists(id))
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

        // POST: api/ExpenseTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ExpenseType>> PostExpenseType(ExpenseType expenseType)
        {
            _context.ExpenseTypes.Add(expenseType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExpenseType", new { id = expenseType.Id }, expenseType);
        }

        // DELETE: api/ExpenseTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpenseType(Guid id)
        {
            var expenseType = await _context.ExpenseTypes.FindAsync(id);
            if (expenseType == null)
            {
                return NotFound();
            }

            _context.ExpenseTypes.Remove(expenseType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExpenseTypeExists(Guid id)
        {
            return _context.ExpenseTypes.Any(e => e.Id == id);
        }
    }
}
