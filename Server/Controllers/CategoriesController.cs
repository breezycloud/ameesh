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
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

		public CategoriesController(AppDbContext context, ILogger<CategoriesController> logger)
		{
			_context = context;
			_logger = logger;
		}
		[HttpPost("paged")]
		public async Task<ActionResult<GridDataResponse<Category>>> PagedCategories(PaginationParameter parameter, CancellationToken cancellationToken)
		{
        GridDataResponse<Category> response = new();
        response!.Data = await _context.Categories.AsNoTracking().OrderByDescending(x => x.ModifiedDate).Skip(parameter.Page).Take(parameter.PageSize).ToListAsync();
        response!.TotalCount = await _context.Categories.CountAsync();
        return response!;
    }

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
		{
			return await _context.Categories.OrderByDescending(x => x.CreatedDate).ToArrayAsync();
		}
		
		[HttpGet("{id}")]
		public async Task<ActionResult<Category?>> GetCategory(Guid id)
		{
			if (_context.Categories == null)
			{
				return NotFound();
			}
			var category =  await _context.Categories.FindAsync(id);
			return category;
		}

		// PUT: api/Category/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPut("{id}")]
		public async Task<IActionResult> PutCategory(Guid id, Category category)
		{
			if (id != category.Id)
			{
				return BadRequest();
			}

			_context.Entry(category).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CategoryExists(id))
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

		// POST: api/Categorys
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

		[HttpPost]
		public async Task<ActionResult<Category>> PostCategory(Category category)
		{
			if (_context.Categories == null)
			{
				return Problem("Entity set 'AppDbContext.Categorys'  is null.");
			}
			_context.Categories.Add(category);
			await _context.SaveChangesAsync();			

			return CreatedAtAction("GetCategory", new { id = category.Id }, category);
		}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.Categories.FindAsync(id);
        if (record is null)
            return NotFound();

        _context.Categories.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }

		public static GridDataResponse<Category> Paginate(IQueryable<Category> source, PaginationParameter parameters)
		{
			int totalItems = source.Count();
			int totalPages = (int)Math.Ceiling((double)totalItems / parameters.PageSize);

			List<Category> items = new();
			items = source
						.OrderByDescending(c => c.CreatedDate)
						.Skip(parameters.Page)
						.Take(parameters.PageSize)
						.ToList();

			return new GridDataResponse<Category>
			{
				Data = items,
				TotalCount = totalItems
			};
		}

		private bool CategoryExists(Guid id)
		{
			return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
