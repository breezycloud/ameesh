using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Helpers;
using Shared.Models.Company;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
//[ResponseCache(CacheProfileName = "Default60")]
public class StoresController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly ILogger<StoresController> _logger;

	public StoresController(AppDbContext context, ILogger<StoresController> logger)
	{
		_context = context;
		_logger = logger;
	}
	[HttpPost("paged")]
	public async Task<ActionResult<GridDataResponse<Store>>> PagedStores(PaginationParameter parameter, CancellationToken cancellationToken)
	{
		IQueryable<Store> query;
		Store[] data = new Store[0];
		data = await _context.Stores.AsNoTracking().ToArrayAsync(cancellationToken);
		query = string.IsNullOrEmpty(parameter.SearchTerm) == true ? data.AsQueryable() : data.Where(x => x.ToString()!.Contains(parameter!.SearchTerm!, StringComparison.InvariantCultureIgnoreCase)).AsQueryable();
		var pagedResult = Paginate(query, parameter);
		return Ok(pagedResult);
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Store>>> GetStores()
	{
		return await _context.Stores.OrderByDescending(x => x.CreatedDate).ToArrayAsync();
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Store?>> GetStore(Guid id)
	{
		if (_context.Stores == null)
		{
			return NotFound();
		}
		var Store = await _context.Stores.FindAsync(id);
		return Store;
	}

	// PUT: api/Store/5
	// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
	[HttpPut("{id}")]
	public async Task<IActionResult> PutStore(Guid id, Store Store)
	{
		if (id != Store.Id)
		{
			return BadRequest();
		}

		_context.Entry(Store).State = EntityState.Modified;

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!StoreExists(id))
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

	// POST: api/Stores
	// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

	[HttpPost]
	public async Task<ActionResult<Store>> PostStore(Store Store)
	{
		if (_context.Stores == null)
		{
			return Problem("Entity set 'AppDbContext.Stores'  is null.");
		}
		_context.Stores.Add(Store);
		await _context.SaveChangesAsync();

		return CreatedAtAction("GetStore", new { id = Store.Id }, Store);
	}


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.Stores.FindAsync(id);
        if (record is null)
            return NotFound();

        _context.Stores.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    public static GridDataResponse<Store> Paginate(IQueryable<Store> source, PaginationParameter parameters)
	{
		int totalItems = source.Count();
		int totalPages = (int)Math.Ceiling((double)totalItems / parameters.PageSize);

		List<Store> items = new();
		items = source
					.OrderByDescending(c => c.CreatedDate)
					.Skip(parameters.Page)
					.Take(parameters.PageSize)
					.ToList();

		return new GridDataResponse<Store>
		{
			Data = items,
			TotalCount = totalItems
		};
	}

	private bool StoreExists(Guid id)
	{
		return (_context.Stores?.Any(e => e.Id == id)).GetValueOrDefault();
	}
}
