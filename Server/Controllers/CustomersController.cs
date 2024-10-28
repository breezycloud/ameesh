using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Helpers;
using Shared.Models.Customers;
using Shared.Models.Products;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[ResponseCache(CacheProfileName = "Default60")]
public class CustomersController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly ILogger<CategoriesController> _logger;

	public CustomersController(AppDbContext context, ILogger<CategoriesController> logger)
	{
		_context = context;
		_logger = logger;
	}

	[HttpPost("paged")]
	public async Task<ActionResult<GridDataResponse<CustomerData>>> PagedCategories(PaginationParameter parameter, CancellationToken cancellationToken)
	{
        GridDataResponse<CustomerData> response = new();
        response!.Data = await _context.Customers.AsNoTracking()
                                                 .AsSplitQuery()
                                                 .Include(o => o.Orders)
                                                 .OrderByDescending(o => o.ModifiedDate)
                                                 .Skip(parameter.Page)
                                                 .Take(parameter.PageSize)
                                                 .Select(n => new CustomerData
                                                 {
                                                     Id = n.Id,
                                                     CustomerName = n.CustomerName,
                                                     PhoneNo = n.PhoneNo,
                                                     ContactAddress = n.ContactAddress,
                                                     TotalSales = n.Orders.Count,
													 IsWalkIn = n.Regular,
                                                     CreatedDate = n.CreatedDate
                                                 })                                                 
                                                 .ToListAsync();
        response!.TotalCount = await _context.Customers.AsNoTracking().CountAsync();
        return response;
	}

	[HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCategories()
	{
		return await _context.Customers.OrderByDescending(x => x.CreatedDate).ToArrayAsync();
	}

	[HttpGet("{id}")]
    public async Task<ActionResult<Customer?>> GetCustomer(Guid id)
	{
		if (_context.Customers == null)
		{
			return NotFound();
		}
        var category = await _context.Customers.AsNoTracking()
                                               .AsSplitQuery()
                                               .Include(t => t.Orders.Take(30))
                                               .ThenInclude(t => t.Store)
                                               .Include(t => t.Orders)
                                               .ThenInclude(p => p.Payments)
                                               .Include(t => t.Orders)
                                               .ThenInclude(t => t.ProductOrders)                                               
                                               .SingleOrDefaultAsync(x => x.Id == id);
		return category;
	}

    [HttpGet("byPhone/{phone}")]
    public async Task<ActionResult<Customer>> ExistCustomer(string phone)
    {
        return await _context.Customers.FirstOrDefaultAsync(x => x.PhoneNo == phone) ?? new Customer();
    }

    // PUT: api/Customers/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
	public async Task<IActionResult> PutCustomer(Guid id, Customer category)
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
			if (!CustomerExists(id))
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

	// POST: api/Customers
	// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

	[HttpPost]
	public async Task<ActionResult<Customer>> PostCustomer(Customer category)
	{
		if (_context.Customers == null)
		{
			return Problem("Entity set 'AppDbContext.Customers'  is null.");
		}
		_context.Customers.Add(category);
		await _context.SaveChangesAsync();

		return CreatedAtAction("GetCustomer", new { id = category.Id }, category);
	}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.Customers.FindAsync(id);
        if (record is null)
            return NotFound();

        _context.Customers.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    public static GridDataResponse<Customer> Paginate(IQueryable<Customer> source, PaginationParameter parameters)
	{
		int totalItems = source.Count();
		int totalPages = (int)Math.Ceiling((double)totalItems / parameters.PageSize);

		List<Customer> items = new();
		items = source
					.OrderByDescending(c => c.CreatedDate)
					.Skip(parameters.Page)
					.Take(parameters.PageSize)
					.ToList();

		return new GridDataResponse<Customer>
		{
			Data = items,
			TotalCount = totalItems
		};
	}

	private bool CustomerExists(Guid id)
	{
		return (_context.Customers?.Any(e => e.Id == id)).GetValueOrDefault();
	}
}
