using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Enums;
using Shared.Helpers;
using Shared.Models.Users;
using Shared.Models.Welfare;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalaryController : ControllerBase
{
    private readonly AppDbContext _context;

    public SalaryController(AppDbContext context)
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
            response.TotalCount = await _context.Salaries.AsNoTracking().Include(x => x.User).AsSplitQuery().Where(x => x.User!.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase) || 
                                x.Amount.ToString()!.Contains(pattern, StringComparison.OrdinalIgnoreCase)).CountAsync(cancellationToken);
            response.Data = await _context.Salaries.AsNoTracking().Include(x => x.User).AsSplitQuery().Where(x => x.User!.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase) || 
                                x.Amount.ToString()!.Contains(pattern, StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).Skip(parameter.Page).Take(parameter.PageSize).Select(x => new WelfareData
                                {
                                    Id = x.Id,
                                    Type = WelfareType.Penalty,
                                    UserId = x.UserId,
                                    Month = x.Month,
                                    Year = x.Year,
                                    StaffName = x.User!.ToString(),
                                    Amount = x.Amount,
                                    CreatedDate = x.CreatedDate
                                }).ToListAsync();
        }
        else
        {
            response.TotalCount = await _context.Salaries.AsNoTracking().CountAsync(cancellationToken);
            response.Data = await _context.Salaries.AsNoTracking().Include(x => x.User).AsSplitQuery().OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).Skip(parameter.Page).Take(parameter.PageSize).Select(x => new WelfareData
                                {
                                    Id = x.Id,
                                    Type = WelfareType.Salary,
                                    UserId = x.UserId,
                                    Month = x.Month,
                                    Year = x.Year,
                                    StaffName = x.User!.ToString(),
                                    Amount = x.Amount,
                                    CreatedDate = x.CreatedDate
                                }).ToListAsync();
        }                
        return response;
	}

    
    [HttpGet("notpaid")]
    public async Task<ActionResult<IEnumerable<StaffDto>?>> GetStaffNotPaid()
    {
        var month = DateTime.Now.Month;
        var year = DateTime.Now.Year;
        var salaries = await _context.Salaries.AsNoTracking().Where(x => x.Month == month && x.Year == year).ToArrayAsync();
        return _context.Users.AsNoTracking().AsEnumerable().AsParallel().Where(u => u.Role != UserRole.Master && !salaries.Any(s => u.Id == s.UserId)).Select(s => new StaffDto
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Role = s.Role,
            CreatedDate = s.CreatedDate,
            ModifiedDate = s.ModifiedDate
        }).ToArray();
    }
    
    // GET: api/Salary/SalMonthYear
    [HttpGet("salmonthyear")]
    public async Task<ActionResult<List<ReportCriteria>>> GetSalMonthYear()
    {
        return await _context.Salaries.GroupBy(x => new { x.Month, x.Year}).Select(d => new ReportCriteria()
        {
            Month = d.Key.Month,
            Year = d.Key.Year
        }).ToListAsync();
    }

    [HttpPost("report")]
	public async Task<ActionResult<IEnumerable<SalaryReportDto>?>> Report(ReportCriteria criteria, CancellationToken cancellationToken)
    {
        var data = await _context.Salaries.AsNoTracking()
                                      .Include(x => x.User)
                                      .ThenInclude(x => x!.SalaryBonus)
                                      .Include(x => x.User)
                                      .ThenInclude(x => x!.SalaryAdvances)
                                      .Include(x => x.User)
                                      .ThenInclude(x => x!.Penalties)
                                      .Where(x => x.Month == criteria.Month && x.Year == criteria.Year)                                      
                                      .Select(x => new SalaryReportDto
                                      {
                                        Staff = x.User!.ToString(),
                                        Amount = x.Amount ?? 0,
                                        Bonus = x.Bonus,
                                        Advance = x.Advance,
                                        Earnings = x.Earnings,
                                        Penalty = x.Penalty,
                                        Deductions = x.Deductions,
                                        Total= x.Total
                                      })                                      
                                      .ToListAsync();
        return data!.OrderByDescending(x => x.Total).ToList();

    }

    // GET: api/Salary
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Salary>>> GetSalaries()
    {
        return await _context.Salaries.ToListAsync();
    }

    // GET: api/Salary/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Salary>> GetSalary(Guid id)
    {
        var salary = await _context.Salaries.FindAsync(id);

        if (salary == null)
        {
            return NotFound();
        }

        return salary;
    }

    // PUT: api/Salary/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSalary(Guid id, Salary salary)
    {
        if (id != salary.Id)
        {
            return BadRequest();
        }

        _context.Entry(salary).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SalaryExists(id))
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

    // POST: api/Salary
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Salary>> PostSalary(Salary salary)
    {
        _context.Salaries.Add(salary);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetSalary", new { id = salary.Id }, salary);
    }

    // DELETE: api/Salary/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSalary(Guid id)
    {
        var salary = await _context.Salaries.FindAsync(id);
        if (salary == null)
        {
            return NotFound();
        }

        _context.Salaries.Remove(salary);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool SalaryExists(Guid id)
    {
        return _context.Salaries.Any(e => e.Id == id);
    }
}
