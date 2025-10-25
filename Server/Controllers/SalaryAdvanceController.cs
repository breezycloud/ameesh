using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Server.Context;
using Server.Pages.Reports.Templates.Welfare;
using Shared.Enums;
using Shared.Helpers;
using Shared.Models.Welfare;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalaryAdvanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public SalaryAdvanceController(AppDbContext context)
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
            response.TotalCount = await _context.SalaryAdvances.AsNoTracking().Include(x => x.User).AsSplitQuery().Where(x => x.User!.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
                                x.Amount.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase)).CountAsync(cancellationToken);
            response.Data = await _context.SalaryAdvances.AsNoTracking().Include(x => x.User).AsSplitQuery().Where(x => x.User!.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
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
            response.TotalCount = await _context.SalaryAdvances.AsNoTracking().CountAsync(cancellationToken);
            response.Data = await _context.SalaryAdvances.AsNoTracking().Include(x => x.User).AsSplitQuery().OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).Skip(parameter.Page).Take(parameter.PageSize).Select(x => new WelfareData
            {
                Id = x.Id,
                UserId = x.UserId,
                Type = WelfareType.Advance,
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

    [HttpPost("advance-report")]
    public async Task<IActionResult> AdvanceReport(ReportCriteria criteria, CancellationToken cancellationToken)
    {
        var data = await _context.SalaryAdvances
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.Month == criteria.Month && x.Year == criteria.Year)
            .Select(x => new SalaryAdvanceReportDto
            {
                Date = x.CreatedDate.ToString("yyyy-MM-dd"),
                Staff = x.User!.ToString(),
                Amount = x.Amount,
                Comment = x.Comment
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(cancellationToken);

        var reportData = new SalaryAdvanceReportData(criteria, data);
        var report = new SalaryAdvanceReport(reportData);
        var pdf = report.GeneratePdf();

        return File(pdf, "application/pdf", $"SalaryAdvance_{criteria.Month}_{criteria.Year}.pdf");
    }


    // GET: api/SalaryAdvance
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalaryAdvance>>> GetSalaryAdvances()
    {
        return await _context.SalaryAdvances.ToListAsync();
    }

    // GET: api/SalaryAdvance/5
    [HttpGet("{id}")]
    public async Task<ActionResult<SalaryAdvance>> GetSalaryAdvance(Guid id)
    {
        var salaryAdvance = await _context.SalaryAdvances.FindAsync(id);

        if (salaryAdvance == null)
        {
            return NotFound();
        }

        return salaryAdvance;
    }

    // PUT: api/SalaryAdvance/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSalaryAdvance(Guid id, SalaryAdvance salaryAdvance)
    {
        if (id != salaryAdvance.Id)
        {
            return BadRequest();
        }

        _context.Entry(salaryAdvance).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SalaryAdvanceExists(id))
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

    // POST: api/SalaryAdvance
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<SalaryAdvance>> PostSalaryAdvance(SalaryAdvance salaryAdvance)
    {
        _context.SalaryAdvances.Add(salaryAdvance);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetSalaryAdvance", new { id = salaryAdvance.Id }, salaryAdvance);
    }

    // DELETE: api/SalaryAdvance/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSalaryAdvance(Guid id)
    {
        var salaryAdvance = await _context.SalaryAdvances.FindAsync(id);
        if (salaryAdvance == null)
        {
            return NotFound();
        }

        _context.SalaryAdvances.Remove(salaryAdvance);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool SalaryAdvanceExists(Guid id)
    {
        return _context.SalaryAdvances.Any(e => e.Id == id);
    }
}
