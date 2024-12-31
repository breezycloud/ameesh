using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Helpers;
using Shared.Models.Products;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemsController(AppDbContext context, ILogger<ItemsController> logger) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<ItemsController> _logger = logger;

    [HttpPost("paged")]
    public async Task<ActionResult<GridDataResponse<Item>>> PagedItems(PaginationParameter parameter, CancellationToken cancellationToken)
    {
        GridDataResponse<Item>? response = new();
        if (parameter.FilterId is null)
        {
            if (!string.IsNullOrEmpty(parameter.SearchTerm))
            {
                var pattern = $"%{parameter.SearchTerm}%";
                response!.Data = await _context.Items.AsNoTracking()
                                                    .AsSplitQuery()
                                                    .Include(x => x.Category)
                                                    .Include(x => x.Brand)
                                                    .Where(x => EF.Functions.ILike(x.ProductName!, pattern) 
                                                    || EF.Functions.ILike(x.Barcode!, pattern)
                                                    || EF.Functions.ILike(x.Category!.CategoryName!, pattern)
                                                    || EF.Functions.ILike(x.Brand!.BrandName!, pattern))
                                                    .OrderByDescending(o => o.CreatedDate)
                                                    .Skip(parameter.Page)
                                                    .Take(parameter.PageSize)
                                                    .ToListAsync(cancellationToken);
                response.TotalCount = await _context.Items.AsNoTracking()
                                                    .AsSplitQuery()
                                                    .Include(x => x.Category)
                                                    .Include(x => x.Brand)
                                                    .Where(x => EF.Functions.ILike(x.ProductName!, pattern) 
                                                    || EF.Functions.ILike(x.Barcode!, pattern)
                                                    || EF.Functions.ILike(x.Category!.CategoryName!, pattern)
                                                    || EF.Functions.ILike(x.Brand!.BrandName!, pattern)).CountAsync();
            }
            else
            {
                response!.Data = await _context.Items.AsNoTracking()
                                .AsSplitQuery()
                                .Include(x => x.Category)
                                .Include(x => x.Brand)
                                .OrderByDescending(o => o.CreatedDate)
                                .Skip(parameter.Page)
                                .Take(parameter.PageSize)
                                .ToListAsync(cancellationToken);
                response.TotalCount = await _context.Items.CountAsync();
            }            
        }
        else
        {
            response!.Data = await _context.Items.AsNoTracking()
                                .AsSplitQuery()
                                .Include(x => x!.Category)
                                .Include(x => x!.Brand)
                                .Where(x => x!.CategoryID == parameter.FilterId)
                                .OrderByDescending(o => o.CreatedDate)
                                .Skip(parameter.Page)
                                .Take(parameter.PageSize)
                                .ToListAsync(cancellationToken);
            response.TotalCount = await _context.Products.Where(x => x!.Item!.CategoryID == parameter.FilterId).CountAsync();
        }
        return response;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> GetItems()
    {
        return await _context.Items.AsNoTracking()
                                          .AsSplitQuery()
                                          .OrderBy(x => x.ProductName)
                                          .ToArrayAsync();
    }

    [HttpGet("byCategory/{id}")]
    public async Task<ActionResult<IEnumerable<Item>>> GetItemsByCategory(Guid id)
    {
        return await _context.Items.AsNoTracking()
                                          .AsSplitQuery()
                                          .Include(x => x.Category)
                                          .Where(x => x.CategoryID == id)
                                          .ToArrayAsync();
    }
    
    [HttpGet("names")]
    public async Task<ActionResult<IEnumerable<string>?>> GetItemsName()
    {
        return await _context.Items.AsNoTracking().Select(x => x.ProductName!).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Item?>> GetItem(Guid id)
    {
        if (_context.Items == null)
        {
            return NotFound();
        }
        var item = await _context.Items.AsNoTracking()
                                       .AsSplitQuery()
                                       .Include(x => x.Category)
                                       .SingleOrDefaultAsync(s => s.Id == id);
        return item;
    }

    // PUT: api/Items/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutItem(Guid id, Item item)
    {
        if (id != item.Id)
        {
            return BadRequest();
        }

        _context.Entry(item).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ItemExists(id))
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

    // POST: api/Items
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

    [HttpPost]
    public async Task<ActionResult<Item>> PostItem(Item item)
    {
        if (_context.Items == null)
        {
            return Problem("Entity set 'AppDbContext.Items'  is null.");
        }
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetItem", new { id = item.Id }, item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.Items.FindAsync(id);
        if (record is null)
            return NotFound();

        _context.Items.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    public static GridDataResponse<Item> Paginate(IQueryable<Item> source, PaginationParameter parameters)
    {
        int totalItems = source.Count();
        int totalPages = (int)Math.Ceiling((double)totalItems / parameters.PageSize);

        List<Item> items = new();
        items = source.OrderByDescending(x => x.ModifiedDate)                    
                      .Skip(parameters.Page)
                      .Take(parameters.PageSize)
                      .ToList();

        return new GridDataResponse<Item>
        {
            Data = items,
            TotalCount = totalItems
        };
    }

    private bool ItemExists(Guid id)
    {
        return (_context.Items?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
