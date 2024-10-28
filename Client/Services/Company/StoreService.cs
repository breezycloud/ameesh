using Shared.Helpers;
using Shared.Models.Company;
using System.Net.Http.Json;

namespace Client.Services.Company;

public interface IStoreService
{
	Task<bool> AddStore(Store model);
	Task<bool> EditStore(Store model);
	Task<bool> DeleteStore(Guid id);
	Task<Store?> GetStore(Guid id);
	Task<Store[]?> GetStores();
	Task<GridDataResponse<Store>?> GetPagedStores(PaginationParameter parameter);
}

public class StoreService : IStoreService
{
	private readonly IHttpClientFactory _client;

	public StoreService(IHttpClientFactory client)
	{
		_client = client;
	}

	public async Task<bool> AddStore(Store model)
	{
		try
		{
			var request = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/stores", model);
			return request.IsSuccessStatusCode;
		}
		catch (Exception)
		{

			throw;
		}
	}

	public async Task<bool> DeleteStore(Guid id)
	{
		try
		{
			var request = await _client.CreateClient("AppUrl").DeleteAsync($"api/stores/{id}");
			return request.IsSuccessStatusCode;
		}
		catch (Exception)
		{

			throw;
		}
	}

	public async Task<bool> EditStore(Store model)
	{
		try
		{
			var request = await _client.CreateClient("AppUrl").PutAsJsonAsync($"api/stores/{model.Id}", model);
			return request.IsSuccessStatusCode;
		}
		catch (Exception)
		{

			throw;
		}
	}

	public async Task<Store?> GetStore(Guid id)
	{
		try
		{
			return await _client.CreateClient("AppUrl").GetFromJsonAsync<Store?>($"api/stores/{id}");
		}
		catch (Exception)
		{

			throw;
		}
	}

	public async Task<Store[]?> GetStores()
	{
		try
		{
			return await _client.CreateClient("AppUrl").GetFromJsonAsync<Store[]?>($"api/stores");
		}
		catch (Exception)
		{

			throw;
		}
	}
	
	public async Task<GridDataResponse<Store>?> GetPagedStores(PaginationParameter parameter)
	{
		try
		{
			var request = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/stores/paged", parameter);
			var response = await request.Content.ReadFromJsonAsync<GridDataResponse<Store>?>();
			return response;
		}
		catch (Exception)
		{

			throw;
		}
	}
}
