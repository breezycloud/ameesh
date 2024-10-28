using System.Net.Http.Json;
using Shared.Helpers;
using Shared.Models.Welfare;

namespace Client.Services.Welfare;


public interface IPenaltyService
{
    Task<bool> AddAsync(Penalty service);
    Task<bool> EditAsync(Penalty service);
    Task<bool> DeleteAsync(Guid id);
    Task<Penalty?> GetAsync(Guid id);
    Task<Penalty[]?> GetAllAsync();
    Task<GridDataResponse<Penalty>?> GetPaged(PaginationParameter parameter);
    Task<GridDataResponse<WelfareData>?> GetPagedData(PaginationParameter parameter);
}
public class PenaltyService(IHttpClientFactory _client) : IPenaltyService, IDisposable
{
    public async Task<bool> AddAsync(Penalty service)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/penalties", service);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/penalties/{id}");
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> EditAsync(Penalty service)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/penalties/{service.Id}", service);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Penalty[]?> GetAllAsync()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Penalty[]?>("api/penalties");            
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Penalty?> GetAsync(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Penalty?>($"api/penalties/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<GridDataResponse<Penalty>?> GetPaged(PaginationParameter parameter)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/penalties/paged", parameter);            
            return await response.Content.ReadFromJsonAsync<GridDataResponse<Penalty>?>();
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<GridDataResponse<WelfareData>?> GetPagedData(PaginationParameter parameter)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/penalties/pagedprojection", parameter);            
            return await response.Content.ReadFromJsonAsync<GridDataResponse<WelfareData>?>();
        }
        catch (Exception)
        {

            throw;
        }    
    }
}