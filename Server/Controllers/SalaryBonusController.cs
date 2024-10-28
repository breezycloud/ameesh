using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Enums;
using Shared.Helpers;
using Shared.Models.Welfare;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalaryBonusController : ControllerBase
{
    private readonly AppDbContext _context;

    public SalaryBonusController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("pagedprojection")]
	public async Task<ActionResult<GridDataResponse<WelfareData>>> PagedCategories(PaginationParameter parameter, CancellationToken cancellationToken)
	{
        GridDataResponse<WelfareData> response = new();
        if (!string.IsNullOrEmpty(parameter.SearchTerm))
        {
            var pattern = $"%{parameter.SearchTerm}%";
            response.TotalCount = await _context.SalaryBonus.AsNoTracking().Include(x => x.User).AsSplitQuery().Where(x => x.User!.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase) || 
                                x.Amount.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase)).CountAsync(cancellationToken);
            response.Data = await _context.SalaryBonus.AsNoTracking().Include(x => x.User).AsSplitQuery().Where(x => x.User!.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase) || 
                                x.Amount.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).Skip(parameter.Page).Take(parameter.PageSize).Select(x => new WelfareData
                                {
                                    Id = x.Id,
                                    UserId = x.UserId,
                                    Type = WelfareType.Bonus,
                                    Month = x.Month,
                                    Year = x.Year,
                                    StaffName = x.User!.ToString(),
                                    Amount = x.Amount,
                                    Comment = x.Comment,
                                    CreatedDate = x.CreatedDate
                                }).ToListAsync();
        }
        else
        {
            response.TotalCount = await _context.SalaryBonus.AsNoTracking().CountAsync(cancellationToken);
            response.Data = await _context.SalaryBonus.AsNoTracking().Include(x => x.User).AsSplitQuery().OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).Skip(parameter.Page).Take(parameter.PageSize).Select(x => new WelfareData
                                {
                                    Id = x.Id,
                                    UserId = x.UserId,
                                    Type = WelfareType.Bonus,
                                    Month = x.Month,
                                    Year = x.Year,
                                    StaffName = x.User!.ToString(),
                                    Amount = x.Amount,
                                    Comment = x.Comment,
                                    CreatedDate = x.CreatedDate
                                }).ToListAsync();
        }                
        return response;
	}

    // GET: api/SalaryBonus
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalaryBonus>>> GetSalaryBonus()
    {
        return await _context.SalaryBonus.ToListAsync();
    }

    // GET: api/SalaryBonus/5
    [HttpGet("{id}")]
    public async Task<ActionResult<SalaryBonus>> GetSalaryBonus(Guid id)
    {
        var salaryBonus = await _context.SalaryBonus.FindAsync(id);

        if (salaryBonus == null)
        {
            return NotFound();
        }

        return salaryBonus;
    }

    // PUT: api/SalaryBonus/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSalaryBonus(Guid id, SalaryBonus salaryBonus)
    {
        if (id != salaryBonus.Id)
        {
            return BadRequest();
        }

        _context.Entry(salaryBonus).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SalaryBonusExists(id))
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

    // POST: api/SalaryBonus
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<SalaryBonus>> PostSalaryBonus(SalaryBonus salaryBonus)
    {
        _context.SalaryBonus.Add(salaryBonus);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetSalaryBonus", new { id = salaryBonus.Id }, salaryBonus);
    }

    // DELETE: api/SalaryBonus/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSalaryBonus(Guid id)
    {
        var salaryBonus = await _context.SalaryBonus.FindAsync(id);
        if (salaryBonus == null)
        {
            return NotFound();
        }

        _context.SalaryBonus.Remove(salaryBonus);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool SalaryBonusExists(Guid id)
    {
        return _context.SalaryBonus.Any(e => e.Id == id);
    }
}
