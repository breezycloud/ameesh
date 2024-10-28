using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Helpers;
using Shared.Models.Products;
using System.Threading;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[ResponseCache(CacheProfileName = "Default60")]
public class BrandsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<BrandsController> _logger;

		public BrandsController(AppDbContext context, ILogger<BrandsController> logger)
		{
			_context = context;
			_logger = logger;
		}
		[HttpPost("paged")]
		public async Task<ActionResult<GridDataResponse<Brand>>> PagedBrands(PaginationParameter parameter, CancellationToken cancellationToken)
		{
        GridDataResponse<Brand> response = new();
        response!.Data = await _context.Brands.AsNoTracking().OrderByDescending(x => x.ModifiedDate).Skip(parameter.Page).Take(parameter.PageSize).ToListAsync();
        response!.TotalCount = await _context.Brands.CountAsync();
        return response!;
    }

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
		{
			return await _context.Brands.OrderByDescending(x => x.CreatedDate).ToArrayAsync();
		}
		
		[HttpGet("{id}")]
		public async Task<ActionResult<Brand?>> GetBrand(Guid id)
		{
			if (_context.Brands == null)
			{
				return NotFound();
			}
			var Brand =  await _context.Brands.FindAsync(id);
			return Brand;
		}

		// PUT: api/Brand/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPut("{id}")]
		public async Task<IActionResult> PutBrand(Guid id, Brand Brand)
		{
			if (id != Brand.Id)
			{
				return BadRequest();
			}

			_context.Entry(Brand).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!BrandExists(id))
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

		// POST: api/Brands
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

		[HttpPost]
		public async Task<ActionResult<Brand>> PostBrand(Brand Brand)
		{
			if (_context.Brands == null)
			{
				return Problem("Entity set 'AppDbContext.Brands'  is null.");
			}
			_context.Brands.Add(Brand);
			await _context.SaveChangesAsync();			

			return CreatedAtAction("GetBrand", new { id = Brand.Id }, Brand);
		}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.Brands.FindAsync(id);
        if (record is null)
            return NotFound();

        _context.Brands.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }

		public static GridDataResponse<Brand> Paginate(IQueryable<Brand> source, PaginationParameter parameters)
		{
			int totalItems = source.Count();
			int totalPages = (int)Math.Ceiling((double)totalItems / parameters.PageSize);

			List<Brand> items = new();
			items = source
						.OrderByDescending(c => c.CreatedDate)
						.Skip(parameters.Page)
						.Take(parameters.PageSize)
						.ToList();

			return new GridDataResponse<Brand>
			{
				Data = items,
				TotalCount = totalItems
			};
		}

		private bool BrandExists(Guid id)
		{
			return (_context.Brands?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
