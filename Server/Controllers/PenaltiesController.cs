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
public class PenaltiesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PenaltiesController(AppDbContext context)
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
            response.TotalCount = await _context.Penalties.AsNoTracking().Include(x => x.User).AsSplitQuery().Where(x => x.User!.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
                                x.Amount.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase)).CountAsync(cancellationToken);
            response.Data = await _context.Penalties.AsNoTracking().Include(x => x.User).AsSplitQuery().Where(x => x.User!.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
                                x.Amount.ToString().Contains(pattern, StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).Skip(parameter.Page).Take(parameter.PageSize).Select(x => new WelfareData
                                {
                                    Id = x.Id,
                                    UserId = x.UserId,
                                    Type = WelfareType.Penalty,
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
            response.TotalCount = await _context.Penalties.AsNoTracking().CountAsync(cancellationToken);
            response.Data = await _context.Penalties.AsNoTracking().Include(x => x.User).AsSplitQuery().OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).Skip(parameter.Page).Take(parameter.PageSize).Select(x => new WelfareData
            {
                Id = x.Id,
                UserId = x.UserId,
                Type = WelfareType.Penalty,
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
    
    [HttpPost("report")]
    public async Task<IActionResult> PenaltyReport(ReportCriteria criteria, CancellationToken cancellationToken)
    {
        var data = await _context.Penalties
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.Month == criteria.Month && x.Year == criteria.Year)
            .Select(x => new PenaltyReportDto
            {
                Date = x.CreatedDate.ToString("yyyy-MM-dd"),
                Staff = x.User!.ToString(),
                Amount = x.Amount,
                Comment = x.Comment
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(cancellationToken);

        var reportData = new PenaltyReportData(criteria, data);
        var report = new PenaltyReport(reportData);
        var pdf = report.GeneratePdf();

        return File(pdf, "application/pdf", $"Penalty_{criteria.Month}_{criteria.Year}.pdf");
    }


    // GET: api/Penalties
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Penalty>>> GetPenalties()
    {
        return await _context.Penalties.ToListAsync();
    }

    // GET: api/Penalties/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Penalty>> GetPenalty(Guid id)
    {
        var penalty = await _context.Penalties.FindAsync(id);

        if (penalty == null)
        {
            return NotFound();
        }

        return penalty;
    }

    // PUT: api/Penalties/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPenalty(Guid id, Penalty penalty)
    {
        if (id != penalty.Id)
        {
            return BadRequest();
        }

        _context.Entry(penalty).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PenaltyExists(id))
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

    // POST: api/Penalties
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Penalty>> PostPenalty(Penalty penalty)
    {
        _context.Penalties.Add(penalty);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPenalty", new { id = penalty.Id }, penalty);
    }

    // DELETE: api/Penalties/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePenalty(Guid id)
    {
        var penalty = await _context.Penalties.FindAsync(id);
        if (penalty == null)
        {
            return NotFound();
        }

        _context.Penalties.Remove(penalty);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PenaltyExists(Guid id)
    {
        return _context.Penalties.Any(e => e.Id == id);
    }
}
