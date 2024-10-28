using ApexCharts;
using Client.Pages.Reports.Templates.Product;
using Microsoft.JSInterop;
using QuestPDF.Fluent;
using Shared.Helpers;
using Shared.Models.Products;
using Shared.Models.Reports;
using System.Data.Common;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Transactions;

namespace Client.Services.Products;

public interface IProductService
{
    Task<bool> AddBrand(Brand brand);
    Task<bool> EditBrand(Brand brand);
    Task<bool> DeleteBrand(Guid id);
    Task<Brand?> GetBrandById(Guid id);
    Task<Brand[]?> GetBrands();
    Task<GridDataResponse<Brand>?> GetPagedBrands(PaginationParameter parameter);    
    Task<bool> AddCategory(Category model);
    Task<bool> EditCategory(Category model);
    Task<bool> DeleteCategory(Guid id);
    Task<Category?> GetCategoryById(Guid id);
    Task<Category[]?> GetCategories();
    Task<GridDataResponse<Category>?> GetPagedCategories(PaginationParameter parameter);    
    Task<bool> AddProduct(Product model);
    Task<bool> EditProduct(Product model);
    Task<bool> EditProduct(ProductByStore model);
    Task<bool> EditProduct(IEnumerable<Product> products);
    Task<bool> DeleteProduct(Guid id);
    Task<Product?> GetProductById(Guid id);
    Task<Product[]?> GetProducts();
    Task<Product[]?> GetProductsByStore(Guid storeId);
    Task<ProductsAvailable[]?> AvailableInDispensary(Guid storeId, CancellationToken token);
    Task<Product[]?> GetExpiryProducts(Guid storeId, string storage);
    Task<GridDataResponse<ProductByStore>?> GetExpiryProductsByStore(Guid storeId, string storage, PaginationParameter parameter);
    Task<Product[]?> GetProductsByCategory(Guid categoryId);
    Task<Product[]?> GetStoreProductsByCategory(Guid storeId, Guid categoryId);
    Task<BulkRestockDispensary[]?> StoreToDispensary(Guid storeId, Guid categoryId);
    Task<GridDataResponse<ProductByStore>?> GetPagedProducts(PaginationParameter parameter);
    Task<GridDataResponse<BulkRestockDispensary>?> GetPagedProductsDispensary(Guid StoreId, PaginationParameter parameter);
    Task<bool> ValidateItemQty(Guid id, Guid storeId, int qty);
    Task<bool> BulkRestockItems(Guid StoreId, string Option, List<BulkRestockDispensary> items);
    Task<bool> AddItem(Item model);
    Task<bool> EditItem(Item model);
    Task<bool> DeleteItem(Guid id);
    Task<Item?> GetItemById(Guid id);
    Task<Item[]?> GetItems();   
    Task<Item[]?> GetItemsByCategory(Guid categoryId);
    Task<GridDataResponse<Item>?> GetPagedItems(PaginationParameter parameter);
    Task<List<string>?> GetItemsName(CancellationToken token);
    Task<bool> AddStock(RestockingModel restocking);
    Task<bool> AddStock(Guid id, string Option, Stock stock);
    Task<bool> EditStock(Guid id, Stock stock);
    Task<GridDataResponse<ExpiryProductData>?> GetExpiryProducts(Guid StoreID, PaginationParameter parameter);
    public Task<int> GetTotalExpiryProducts(Guid id);
    public Task<int> GetTotalDispensaryExpiryProducts(Guid id);
    public Task<int> GetTotalStoreExpiryProducts(Guid id);
    public Task GetProductsList();
    Task StockAudit(Guid id);
    public IAsyncEnumerable<List<ProductItems>>? GetProductsListAsync();
}

public class ProductService : IProductService
{
    private readonly IHttpClientFactory _client;
    private readonly IJSRuntime _js;
    public ProductService(IHttpClientFactory client, IJSRuntime js)
    {
        _client = client;
        _js = js;
    }

    public async Task GetProductsList()
    {
        var model = new ProductReportTemplate { StoreName = "Ameesh Luxury" };
        var result = GetProductsListAsync().GetAsyncEnumerator();
        while (await result.MoveNextAsync())
        {
            var data = result.Current;

            await Parallel.ForEachAsync(data, async (item, token) =>
            {
                // do somethings
                model.Items.Add(item);
                await Task.Delay(0);
            });
        }

        try
        {
            var doc = new ProductReport(model);
            var content = doc.GeneratePdf();
            await _js.InvokeAsync<object>("exportFile", "Products.pdf", Convert.ToBase64String(content));
        }
        catch (Exception ex)
        {
            Console.WriteLine("failed to generate report {0}", ex.Message);

        }

    }


    public async Task StockAudit(Guid id)
    {        
        var response = await _client.CreateClient("AppUrl").GetFromJsonAsync<UserSoldProduct?>($"api/orderitems/stockoutreport/{id}");
        try
        {
            var doc = new StockAuditReport(response);
            var content = doc.GeneratePdf();
            await _js.InvokeAsync<object>("exportFile", "Stock Audit.pdf", Convert.ToBase64String(content));
        }
        catch (Exception ex)
        {
            Console.WriteLine("failed to generate report {0}", ex.Message);

        }

    }



    public IAsyncEnumerable<List<ProductItems>?>? GetProductsListAsync()
    {
        return _client.CreateClient("AppUrl").GetFromJsonAsAsyncEnumerable<List<ProductItems>?>("api/products/productlist");
    }

    public async Task<List<string>?> GetItemsName(CancellationToken token)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<List<string>?>("api/items/names", token);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> AddCategory(Category model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/categories", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> AddProduct(Product model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/products", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> DeleteCategory(Guid id)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/categories/{id}");
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> DeleteProduct(Guid id)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/products/{id}");
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> EditCategory(Category model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/categories/{model.Id}", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> EditProduct(Product model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/products/{model.Id}", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> EditProduct(ProductByStore model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/products/stockandprice", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }


    public async Task<bool> EditProduct(IEnumerable<Product> products)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync($"api/products/bulkstorerestock", products);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Category[]?> GetCategories()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Category[]?>("api/categories");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Category?> GetCategoryById(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Category?>($"api/categories/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }

	public async Task<GridDataResponse<Category>?> GetPagedCategories(PaginationParameter parameter)
	{
		try
		{
			var request = await  _client.CreateClient("AppUrl").PostAsJsonAsync($"api/categories/paged",parameter);
            var response = await request.Content.ReadFromJsonAsync<GridDataResponse<Category>?>();
            return response;
		}
		catch (Exception)
		{

			throw;
		}
	}

	public async Task<GridDataResponse<ProductByStore>?> GetPagedProducts(PaginationParameter parameter)
	{
		try
		{
			var request = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/products/paged", parameter);
			var response = await request.Content.ReadFromJsonAsync<GridDataResponse<ProductByStore>?>();
			return response!;
		}
		catch (Exception)
		{
			throw;
		}
	}

    public async Task<GridDataResponse<BulkRestockDispensary>?> GetPagedProductsDispensary(Guid StoreId, PaginationParameter parameter)
	{
		try
		{
			var request = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/products/ProductsToRestock/{StoreId}", parameter);
            var response = await request.Content.ReadFromJsonAsync<GridDataResponse<BulkRestockDispensary>?>();
			return response!;
		}
		catch (Exception)
		{
			throw;
		}
	}

	public async Task<Product?> GetProductById(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Product?>($"api/products/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Product[]?> GetProducts()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Product[]?>("api/products");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Product[]?> GetProductsByCategory(Guid categoryId)
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<Product[]?>($"api/products/byCategory/{categoryId}");
    }

    public async Task<Product[]?> GetProductsByStore(Guid storeId)
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<Product[]?>($"api/products/byBranch/{storeId}");
    }
    public async Task<ProductsAvailable[]?> AvailableInDispensary(Guid storeId, CancellationToken token)
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<ProductsAvailable[]?>($"api/products/AvailableInDispensary/{storeId}", token);
    }

    public async Task<GridDataResponse<ProductByStore>?> GetExpiryProductsByStore(Guid storeId, string storage, PaginationParameter parameter)
    {
        var response = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/products/expiry?Storage={storage}&StoreId={storeId}", parameter);
        return await response.Content.ReadFromJsonAsync<GridDataResponse<ProductByStore>?>();
    }
    
    public async Task<Product[]?> GetExpiryProducts(Guid storeId, string storage)
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<Product[]?>($"api/products/expiry?Storage={storage}&StoreId={storeId}");
    }
    public async Task<Product[]?> GetStoreProductsByCategory(Guid storeId, Guid categoryId)
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<Product[]?>($"api/products/StoreProductsByCategory?StoreId={storeId}&CategoryId={categoryId}");
    }
    
    public async Task<BulkRestockDispensary[]?> StoreToDispensary(Guid storeId, Guid categoryId)
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<BulkRestockDispensary[]?>($"api/products/RestockDispensary?StoreId={storeId}&CategoryId={categoryId}");
    }

    public async Task<bool> AddItem(Item model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/items", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<bool> EditItem(Item model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/items/{model.Id}", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }

    }
    public async Task<bool> DeleteItem(Guid id)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/items/{id}");
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<Item?> GetItemById(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Item?>($"api/items/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<Item[]?> GetItems()
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<Item[]?>("api/items");
    }    
    public async Task<Item[]?> GetItemsByCategory(Guid categoryId)
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<Item[]?>($"api/items/byCategory/{categoryId}");
    }
    public async Task<GridDataResponse<Item>?> GetPagedItems(PaginationParameter parameter)
    {
        try
        {
            var request = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/items/paged", parameter);
            var response = await request.Content.ReadFromJsonAsync<GridDataResponse<Item>?>();
            return response;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> AddStock(RestockingModel restocking)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/products/restock", restocking);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }
    
    public async Task<bool> AddStock(Guid id, string Option,  Stock stock)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/products/stock?option={Option}&id={id}", stock);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<bool> EditStock(Guid id, Stock stock)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/products/editstock/{id}", stock);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> ValidateItemQty(Guid id, Guid storeId, int qty)
    {
        var response = await _client.CreateClient("AppUrl").GetFromJsonAsync<bool>($"api/products/validateqty?id={id}&storeId={storeId}&qty={qty}");
        return response;
    }

    public async Task<bool> BulkRestockItems(Guid StoreId, string Option, List<BulkRestockDispensary> items)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/products/bulkrestockitems?StoreId={StoreId}&Option={Option}", items);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<GridDataResponse<ExpiryProductData>?> GetExpiryProducts(Guid StoreID, PaginationParameter parameter)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/products/expiryproducts/{StoreID}", parameter);
            return await response.Content.ReadFromJsonAsync<GridDataResponse<ExpiryProductData>?>();
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<int> GetTotalExpiryProducts(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<int>($"api/products/totalexpiryproducts/{id}");
        }
        catch (Exception)
        {

            throw;
        }        
    }

    public async Task<int> GetTotalDispensaryExpiryProducts(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<int>($"api/products/dispensaryexpiryproducts/{id}");
        }
        catch (Exception)
        {

            throw;
        }        
    }

    public async Task<int> GetTotalStoreExpiryProducts(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<int>($"api/products/storeexpiryproducts/{id}");
        }
        catch (Exception)
        {

            throw;
        }        
    }

    public async Task<bool> AddBrand(Brand model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/brands", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> EditBrand(Brand model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/brands/{model.Id}", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public Task<bool> DeleteBrand(Guid id)
    {
        throw new NotImplementedException();
    }
    public Task<Brand?> GetBrandById(Guid id)
    {
        throw new NotImplementedException();
    }
    public async Task<Brand[]?> GetBrands()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Brand[]?>("api/brands");
        }
        catch (Exception)
        {

            throw;
        }
    }
    public Task<GridDataResponse<Brand>?> GetPagedBrands(PaginationParameter parameter)
    {
        throw new NotImplementedException();
    }
}
