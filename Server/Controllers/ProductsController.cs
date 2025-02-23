using System.Collections.Generic;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Threading;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Server.Context;
using Server.Pages.Reports.Templates.Product;
using Shared.Enums;
using Shared.Helpers;
using Shared.Models.Products;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly ILogger<ProductsController> _logger;

	public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
	{
		_context = context;
		_logger = logger;
	}

	[HttpPost("paged")]
	public async Task<ActionResult<GridDataResponse<ProductByStore>?>> PagedProducts(PaginationParameter parameter, CancellationToken cancellationToken)
	{
        GridDataResponse<ProductByStore>? response = new();
		if (string.IsNullOrEmpty(parameter.SearchTerm))
		{
            response!.Data = await _context.Products.AsNoTracking()
                                                    .AsSplitQuery()
                                                    .Include(store => store.Store)
                                                    .Include(item => item.Item)                 
                                                    .ThenInclude(b => b.Brand)
                                                    .Include(item => item.Item)
                                                    .ThenInclude(category => category!.Category)
                                                    .Include(items => items.OrderItems)
                                                    .Where(x => x.StoreId == parameter!.FilterId)
                                                    .OrderByDescending(d => d.CreatedDate)
                                                    .Skip(parameter.Page)
                                                    .Take(parameter.PageSize)                                                    
                                                    .Select(x => new ProductByStore
                                                    {
                                                        Id = x.Id,
                                                        StoreName = x.Store!.BranchName,
                                                        CategoryName = x.Item!.Category!.CategoryName,
                                                        ProductName = x.Item!.ProductName,
                                                        Price = x.SellPrice.GetValueOrDefault(),
                                                        DispensaryQuantity = x.DispensaryQuantity,
                                                        MarkupType = x.MarkupType,
                                                        MarkupAmount = x.MarkupAmount,
                                                        MarkupPercentage = x.MarkupPercentage,
                                                        StoreQuantity = x.StoreQuantity,
                                                        Dispensary = x.Dispensary,
                                                        Stocks = x.Stocks,
                                                        CreatedDate = x.CreatedDate,
                                                        ModifiedDate = x.ModifiedDate
                                                    }).ToListAsync(cancellationToken);
            response.TotalCount = await _context.Products.Where(x => x.StoreId == parameter!.FilterId).CountAsync();
        }		
		else
		{
            var pattern = $"%{parameter.SearchTerm}%";
            response!.Data = await _context.Products.AsNoTracking()
                                                    .AsSplitQuery()
                                                    .Include(store => store.Store)
                                                    .Include(item => item.Item)
                                                    .ThenInclude(b => b.Brand)
                                                    .Include(item => item.Item)
                                                    .ThenInclude(category => category!.Category)
                                                    .Include(items => items.OrderItems)
                                                    .Where(x => x.StoreId == parameter!.FilterId && EF.Functions.ILike(x!.Item!.Brand!.BrandName!, pattern) || EF.Functions.ILike(x!.Item!.ProductName!, pattern))
                                                    .OrderByDescending(d => d.CreatedDate)
                                                    .Skip(parameter.Page)
                                                    .Take(parameter.PageSize)
                                                    .Select(x => new ProductByStore
                                                    {
                                                        Id = x.Id,
                                                        StoreName = x.Store!.BranchName,
                                                        CategoryName = x.Item!.Category!.CategoryName,
                                                        ProductName = x.Item!.ProductName,
                                                        Price = x.SellPrice.GetValueOrDefault(),
                                                        MarkupType = x.MarkupType,
                                                        MarkupAmount = x.MarkupAmount,
                                                        MarkupPercentage = x.MarkupPercentage,
                                                        DispensaryQuantity = x.DispensaryQuantity,
                                                        StoreQuantity = x.StoreQuantity,
                                                        Dispensary = x.Dispensary,
                                                        Stocks = x.Stocks,
                                                        CreatedDate = x.CreatedDate,
                                                        ModifiedDate = x.ModifiedDate
                                                    })
                                                    .ToListAsync(cancellationToken);
            response.TotalCount = await _context.Products.Where(x => x.StoreId == parameter!.FilterId).CountAsync();
        }
        return response;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
	{
		return await _context.Products.AsNoTracking()
                                      .AsSplitQuery()
                                      .Include(store => store.Store)
                                      .Include(item => item.Item)
                                      .ThenInclude(b => b.Brand)
                                      .Include(item => item.Item)
                                      .ThenInclude(category => category!.Category)
                                      .Include(items => items.OrderItems.Where(x => x.Status == OrderStatus.Pending || x.Status == OrderStatus.Canceled))
                                      .OrderByDescending(x => x.CreatedDate).ToArrayAsync();
	}

    [HttpPut("stockandprice")]
    public async Task<ActionResult<bool>> EditProduct(ProductByStore product)
    {
        var item = await _context.Products.FindAsync(product.Id);
        if (item is not null)
            await _context.Items.Where(x => x.Id == item.ItemId).ExecuteUpdateAsync(s => s.SetProperty(p => p.ProductName, product.ProductName));

        var rowsAffected = await _context.Products.Where(x => x.Id == product.Id)
                                                  .ExecuteUpdateAsync(s => s.SetProperty(p => p.SellPrice, product.Price)
                                                  .SetProperty(p => p.MarkupType, product.MarkupType)
                                                  .SetProperty(p => p.MarkupAmount, product.MarkupAmount)
                                                  .SetProperty(p => p.MarkupPercentage, product.MarkupPercentage)
                                                  .SetProperty(p => p.Stocks, product.Stocks));
        return rowsAffected > 0 ? true : false;
    }
    
    [HttpPost("expiryproducts/{StoreID}")]
	public async Task<ActionResult<GridDataResponse<ExpiryProductData>?>> GetExpiryProducts(Guid StoreID, PaginationParameter parameter)
	{
        List<ExpiryProductData> expiryProducts = new();
		var products = await _context.Products.AsNoTracking()
                                      .AsSplitQuery()
                                      .Include(store => store.Store)    
                                      .Where(x => x.StoreId == StoreID)
                                      .OrderByDescending(x => x.CreatedDate)
                                      .Skip(parameter.Page)
                                      .Take(parameter.PageSize)
                                      .ToArrayAsync();       

        foreach ( var product in products)
        {
            var stock = product.Stocks.Where(x => x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).FirstOrDefault();
            if (stock is null)
                continue;

            expiryProducts.Add(new()
            {
                ProductId = product.Id,
                StoreId = product.StoreId,
                SellPrice = product.SellPrice.GetValueOrDefault(),
                ProductName = product.Item!.ProductName,
                ExpiryDate = stock.ExpiryDate!.Value,
            });
        }

        return new GridDataResponse<ExpiryProductData>
        {
            Data = expiryProducts,
            TotalCount = products.SelectMany(x => x.Stocks).Where(x => x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).Count()
        };

    }
    
    [HttpGet("StoreProductsByCategory")]
	public async Task<ActionResult<IEnumerable<Product>>> GetProducts(Guid StoreId, Guid CategoryId)
	{
		return await _context.Products.AsNoTracking()
                                      .AsSplitQuery()
                                      .Include(x => x.Item)                                      
                                      .ThenInclude(x => x.Category)
                                      .Where(x => x.StoreId == StoreId && x.Item!.CategoryID == CategoryId)
                                      .OrderByDescending(x => x.CreatedDate)
                                      .ToArrayAsync();
	}
    
    [HttpPost("ProductsToRestock/{StoreId}")]
	public async Task<ActionResult<GridDataResponse<BulkRestockDispensary>?>> GetProductsToRestock(Guid StoreId, PaginationParameter parameter)
	{
        GridDataResponse<BulkRestockDispensary>? response = new();

        var data = await _context.Products.AsNoTracking()
                                                    .AsSplitQuery()
                                                    .Include(item => item.Item)
                                                    .Include(items => items.OrderItems)
                                                    .Where(x => x!.StoreId == StoreId)
                                                    .OrderByDescending(x => x.CreatedDate)
                                                    .Skip(parameter.Page)
                                                    .Take(parameter.PageSize)                                                    
                                                    .ToListAsync();
        response.Data = data.Select(x => new BulkRestockDispensary
        {
            Id = x.Id,
            ItemId = x.ItemId,
            StoreId = x.StoreId,
            ProductName = x.Item!.ProductName,
            Price = x.SellPrice.GetValueOrDefault(),
            CurrentDispensary = x.DispensaryQuantity,
            QuantitySold = x.QuantitySold,
            StoreQuantity = x.StoreQuantity,
            ExpiryDate = x.Dispensary.Select(e => e.ExpiryDate).FirstOrDefault().GetValueOrDefault(),
            CurrentStoreQuantity = x.StoreQuantity          
        }).ToList();
        response.TotalCount = await _context.Products.Where(x => x!.StoreId == StoreId).CountAsync();
        return response;
	}

    [HttpPost("bulkrestockitems")]
    public async Task<ActionResult> BulkRestockItems(Guid StoreId, string Option, List<BulkRestockDispensary> items)
    {
        try
        {
            foreach (var item in items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(x => x.StoreId == StoreId && x.Id == item.Id);
                if (Option == "Dispensary")
                {                    
                    product!.Dispensary!.FirstOrDefault(x => x.id == item.StockId)!.Quantity += item!.NewQuantity!.Value;
                }
                else
                {
                    product!.Stocks.Add(new Stock
                    {
                        id = Guid.NewGuid(),
                        Date = item.Date,
                        Quantity = item.NewQuantity
                    });
                }
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        return Ok();
    }
    
    [HttpPost("restock")]
    public async Task<ActionResult> BulkRestockItems(RestockingModel restocking)
    {
        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.StoreId == restocking.StoreID && x.Id == restocking.ProductId);
            if (restocking.Option == "Dispensary")
            {
                product!.Dispensary!.FirstOrDefault(x => x.id == restocking.StockID)!.Quantity += restocking!.NewQty!.Value;
                product!.Stocks.Where(x => x.id == restocking.StockID)!.First()!.Quantity -= restocking!.NewQty!.Value;
            }
            else
            {
                product!.Stocks.Where(x => x.id == restocking.StockID)!.First()!.Quantity += restocking!.NewQty!.Value;
            }
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        return Ok();
    }
    
    [HttpPost("expiry")]
    public async Task<ActionResult<GridDataResponse<ProductByStore>>> GetExpiryProducts([FromQuery]string Storage, Guid StoreId, PaginationParameter parameter)
    {
        
        List<Product> products = new();
        int capacity = 0;
        List<ExpiryProductData> expiryProducts = new(capacity);
        GridDataResponse<ProductByStore> response = new();
        response.Data = new List<ProductByStore>();
        try
        {            

            Console.WriteLine(Storage);
            if (Storage == "Dispensary")                
            {
                products = _context.Products.Where(x => x.StoreId == StoreId)
                                              .AsNoTracking()
                                              .AsSplitQuery()
                                              .Include(store => store.Store)
                                              .Include(item => item.Item)
                                              .ThenInclude(b => b.Brand)
                                              .Include(item => item.Item)
                                              .ThenInclude(category => category!.Category)
                                              .OrderBy(x => x.CreatedDate)
                                              .AsParallel()
                                              .AsEnumerable()
                                              .Where(x => x.Dispensary.Any(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90))
                                              .Skip(parameter.Page)
                                              .Take(parameter.PageSize)
                                              .ToList();
                response.TotalCount = _context.Products.AsParallel().SelectMany(x => x.Dispensary).Where(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).Count();
                foreach (var product in products.AsParallel())
                {
                    if (product.Dispensary.AsParallel().Where(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).Any())
                    {
                        var expired = product.Dispensary.AsParallel().Where(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90);
                        response!.Data!.Add(new ProductByStore
                        {
                            Id = product.Id,
                            StoreName = product.Store!.BranchName,
                            BrandName = product.Item!.Brand!.BrandName,
                            CategoryName = product.Item!.Category!.CategoryName,
                            ProductName = product.Item!.ProductName,
                            Price = product.SellPrice.GetValueOrDefault(),
                            DispensaryQuantity = product.DispensaryQuantity,
                            StoreQuantity = product.StoreQuantity,
                            Dispensary = expired.ToList(),
                            CreatedDate = product.CreatedDate,
                            ModifiedDate = product.ModifiedDate
                        });
                    }                      
                }
            }
            else
            {
                products = _context.Products.Where(x => x.StoreId == StoreId)
                                              .AsNoTracking()
                                              .AsSplitQuery()
                                              .Include(store => store.Store)
                                              .Include(item => item.Item)
                                              .ThenInclude(b => b.Brand)
                                              .Include(item => item.Item)
                                              .ThenInclude(category => category!.Category)
                                              .OrderBy(x => x.CreatedDate)
                                              .AsParallel()
                                              .AsEnumerable()
                                              .Where(x => x.Stocks.Any(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90))
                                              .Skip(parameter.Page)
                                              .Take(parameter.PageSize)
                                              .ToList();
                response.TotalCount = _context.Products.AsParallel().SelectMany(x => x.Stocks).Where(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).Count();
                foreach (var product in products.AsParallel())
                {
                    if (product.Stocks.AsParallel().Where(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).Any())
                    {
                        var expired = product.Stocks.AsParallel().Where(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90);
                        response!.Data!.Add(new ProductByStore
                        {
                            Id = product.Id,
                            StoreName = product.Store!.BranchName,
                            BrandName = product.Item!.Brand!.BrandName,
                            CategoryName = product.Item!.Category!.CategoryName,
                            ProductName = product.Item!.ProductName,
                            Price = product.SellPrice.GetValueOrDefault(),
                            DispensaryQuantity = product.DispensaryQuantity,
                            StoreQuantity = product.StoreQuantity,
                            Stocks = expired.ToList(),
                            CreatedDate = product.CreatedDate,
                            ModifiedDate = product.ModifiedDate
                        });
                    }                      
                }
            }           
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        return Ok(response);
    }

    [HttpGet("totalexpiryproducts/{id}")]
    public async Task<int> GetTotalExpiryProducts(Guid id)
    {
        int dispensary = await GetTotalDispensaryExpiryProducts(id);
        int store = await GetTotalStoreExpiryProducts(id);
        return dispensary + store;
    }
    
    [HttpGet("dispensaryexpiryproducts/{id}")]
    public async Task<int> GetTotalDispensaryExpiryProducts(Guid id)
    {
        return _context.Products.AsNoTracking().AsParallel().Where(x => x.StoreId == id).SelectMany(x => x.Dispensary).AsEnumerable().Where(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).Count();
    }
    [HttpGet("storeexpiryproducts/{id}")]
    public async Task<int> GetTotalStoreExpiryProducts(Guid id)
    {
        return _context.Products.AsNoTracking().AsParallel().Where(x => x.StoreId == id).SelectMany(x => x.Stocks).AsEnumerable().Where(x => x.ExpiryDate.HasValue && x.ExpiryDate!.Value!.Date.Subtract(DateTime.UtcNow.Date).Days <= 90).Count();
    }

    [HttpPost("bulkstorerestock")]
    public async Task<ActionResult> BulkStoreRestockItems(Guid StoreId, IEnumerable<Product> products)
    {
        try
        {
            foreach (var product in products)
            {
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        return Ok();
    }

	[HttpGet("{id}")]
	public async Task<ActionResult<Product?>> GetProduct(Guid id)
	{
		if (_context.Products == null)
		{
			return NotFound();
		}
        var Product = await _context.Products.AsNoTracking()
                                             .AsSplitQuery()
                                             .Include(store => store.Store)
                                             .Include(item => item.Item)
                                             .ThenInclude(category => category!.Category)
                                             .Include(items => items.ReturnedProducts)
                                             .Include(x => x.OrderItems)
                                             .SingleOrDefaultAsync(x => x.Id == id);
		return Product;
	}
    
    [HttpGet("validateqty")]
	public async Task<ActionResult<bool>> ValidateQty(Guid id, Guid storeId, Guid stockId, int qty)
	{
		if (_context.Products == null)
		{
			return NotFound();
		}
        var product = await _context.Products.Where(x => x.Id == id && x.StoreId == storeId).FirstOrDefaultAsync();
        var IsValid = product!.Dispensary.Any(s => s.id == stockId && s.Quantity >= qty);
        return IsValid;
	}
    
    public async IAsyncEnumerable<List<ProductItems>> GetProductsList()
    {
        int DefaultPageSize = 100;
        var totalCount = await _context.Products.CountAsync();

        for (var i = 0; i <= Math.Ceiling(totalCount / Convert.ToDecimal(DefaultPageSize)); i++)
        {
            var data = await _context.Products.Include(item => item.Item)
                                .ThenInclude(b => b.Brand)
                                .Include(item => item.Item)
                                .ThenInclude(category => category!.Category)
                                .OrderBy(x => x.Item!.ProductName)
                                .Skip((i) * DefaultPageSize)
                                .Take(DefaultPageSize)
                                .ToListAsync();

            foreach (var product in data)
            {
                if (product.Total <= 0)                
                    continue;

                foreach (var stock in product.Stocks)
                {
                    yield return new List<ProductItems>()
                    {
                        new ProductItems
                        {
                            BrandName = product.Item!.Brand!.BrandName,
                            CategoryName = product.Item!.Category!.CategoryName,
                            ProductName = product.Item!.ProductName,
                            CostPrice = stock.BuyPrice.GetValueOrDefault(),
                            SellPrice = product.SellPrice.GetValueOrDefault(),
                            DispensaryQuantity = product.DispensaryQuantity,
                            StoreQuantity = stock.Quantity.GetValueOrDefault(),
                        }
                    };
                }
            }
        }        
    }

    [HttpGet("productlist")]
    public async Task<ActionResult> Export()
    {
        var model = new ProductReportTemplate { StoreName = "Ameesh Luxury" };
        await foreach (var item in GetProductsList())
        {
            model.Items.AddRange(item);
        }
        var doc = new ProductReport(model);
        var pdf = doc.GeneratePdf();

        return File(pdf, "application/pdf");
    }

    // GET: api/Products/5
    [HttpGet("byBranch/{id}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByBranch(Guid id)
    {
        var products = await _context.Products.AsNoTracking()
                                              .AsSplitQuery()
                                              .Include(b => b.Store)
                                              .Include(x => x.Item)
                                              .ThenInclude(x => x.Category)
                                              .Include(items => items.OrderItems)
                                              .Where(x => x.StoreId == id)
                                              .OrderByDescending(x => x.ModifiedDate)
                                              .ToListAsync();

        if (products == null)
        {
            return NotFound();
        }

        return products;
    }
    
    // GET: api/Products/5
    [HttpGet("AvailableInDispensary/{id}")]
    public ActionResult<IEnumerable<ProductsAvailable>> AvailableInDispensary(Guid id, CancellationToken token)
    {
        var products = _context.Products.AsNoTracking()
                                              .AsSplitQuery()
                                              .Include(x => x.Item)                                              
                                              .AsEnumerable()
                                              .AsParallel()
                                              .Where(x => x.StoreId == id)
                                              .OrderByDescending(x => x.ModifiedDate)
                                              .Select(p=> new ProductsAvailable
                                              { 
                                                  Id = p.Id,
                                                  StoreId = p.StoreId,
                                                  Barcode = p.Item!.Barcode,
                                                  ProductName = p.Item!.ProductName,
                                                  SellPrice = p.SellPrice,
                                                  Dispensary = p.Dispensary.Where(x => x.Quantity.GetValueOrDefault() > 0).ToList()
                                              })
                                              .WithCancellation(token)
                                              .ToList();

        if (products == null)
        {
            return NotFound();
        }

        return products.AsParallel().Where(x => x.Dispensary.Any(d => d.Quantity.GetValueOrDefault() > 0));
    }

    // PUT: api/Product/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
	public async Task<IActionResult> PutProduct(Guid id, Product Product)
	{
		if (id != Product.Id)
		{
			return BadRequest();
		}

		_context.Entry(Product).State = EntityState.Modified;

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!ProductExists(id))
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

	// POST: api/Products
	// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

	[HttpPost]
	public async Task<ActionResult<Product>> PostProduct(Product Product)
	{
		if (_context.Products == null)
		{
			return Problem("Entity set 'AppDbContext.Products'  is null.");
		}
		_context.Products.Add(Product);
		await _context.SaveChangesAsync();

		return CreatedAtAction("GetProduct", new { id = Product.Id }, Product);
	}
    
    // POST: api/Products/Stock
	// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

	[HttpPost("stock")]
	public async Task<ActionResult<Stock>> PostStock([FromQuery] string option, Guid id, Stock stock)
	{
		if (_context.Products == null)
		{
			return Problem("Entity set 'AppDbContext.Products'  is null.");
		}
		var product = await _context.Products.FindAsync(id);
        if (product is null)
            return BadRequest();

        if (option == "Store")
            product.Stocks.Add(stock);
        else
        {            
            if (product.Dispensary.Any(x => x.id == stock.id))
                product.Dispensary.FirstOrDefault(x => x.id == stock.id)!.Quantity += stock.Quantity;
            else
                product.Dispensary.Add(stock);

            product.Stocks.FirstOrDefault(x => x.id == stock.id)!.Quantity -= stock.Quantity;
        }
        _context.Entry(product).State = EntityState.Modified;
		await _context.SaveChangesAsync();

        return Ok();
	}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.Products.FindAsync(id);
        if (record is null)
            return NotFound();

        _context.Products.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    public static GridDataResponse<Product> Paginate(IQueryable<Product> source, PaginationParameter parameters)
	{
		int totalItems = source.Count();
		int totalPages = (int)Math.Ceiling((double)totalItems / parameters.PageSize);

		List<Product> items = new();
		items = source
					.OrderByDescending(c => c.CreatedDate)
					.Skip(parameters.Page)
					.Take(parameters.PageSize)
					.ToList();

		return new GridDataResponse<Product>
		{
			Data = items,
			TotalCount = totalItems
		};
	}

	private bool ProductExists(Guid id)
	{
		return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
	}
}
