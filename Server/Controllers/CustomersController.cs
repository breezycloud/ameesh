using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Server.Context;
using Server.Pages.Reports.Templates.Customers;
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

	[HttpPost("print")]
	public async Task<IActionResult> PrintCustomers(CustomerFilter filter)
	{
		if (filter == null)
		{
			return BadRequest("Filter is required!");
		}

		IQueryable<Customer> query = _context.Customers.AsNoTracking();

		switch (filter.Type)
		{
			case "WithAddress":
				query = query.Where(c => !string.IsNullOrEmpty(c.ContactAddress));
				break;
			case "WithoutAddress":
				query = query.Where(c => string.IsNullOrEmpty(c.ContactAddress));
				break;
			default:
				break;
		}

		var customers = await query.ToListAsync();
		var doc = new CustomerReport(customers, filter.FilterType);
		var content = doc.GeneratePdf();
		return File(content, "application/pdf", $"Customer Report {DateTime.Now:yyyyMMddHHmmss}.pdf");
	}

	[HttpPost("paged")]
	public async Task<ActionResult<GridDataResponse<CustomerData>>> PagedCategories(PaginationParameter parameter, CancellationToken cancellationToken)
	{
        GridDataResponse<CustomerData> response = new();
		if (!string.IsNullOrEmpty(parameter.SearchTerm))
		{
			parameter.SearchTerm = $"%{parameter.SearchTerm}%";
			response!.Data = await _context.Customers.AsNoTracking()
                                                 .AsSplitQuery()
                                                 .Include(o => o.Orders)
												 .Where(x => EF.Functions.ILike(x.CustomerName!, parameter.SearchTerm) ||
														EF.Functions.ILike(x.PhoneNo!, parameter.SearchTerm) ||
														EF.Functions.ILike(x.ContactAddress!, parameter.SearchTerm))
												 .Skip(parameter.Page)
                                                 .Take(parameter.PageSize)
                                                 .OrderByDescending(o => o.CreatedDate)
												 .ThenByDescending(o => o.ModifiedDate)
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
			response!.TotalCount = await _context.Customers.AsNoTracking()
														.Where(x => EF.Functions.ILike(x.CustomerName!, parameter.SearchTerm) ||
														EF.Functions.ILike(x.PhoneNo!, parameter.SearchTerm) ||
														EF.Functions.ILike(x.ContactAddress!, parameter.SearchTerm))
														.CountAsync();									
		}
        else
		{
			response!.Data = await _context.Customers.AsNoTracking()
                                                 .AsSplitQuery()
                                                 .Include(o => o.Orders)
                                                 .Skip(parameter.Page)
                                                 .Take(parameter.PageSize)
                                                 .OrderByDescending(o => o.CreatedDate)
												 .ThenByDescending(o => o.ModifiedDate)
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
		}
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
	public async Task<IActionResult> PutCustomer(Guid id, Customer customer)
	{
		if (id != customer.Id)
		{			
			return BadRequest();
		}		
		_context.Entry(customer).State = EntityState.Modified;

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
