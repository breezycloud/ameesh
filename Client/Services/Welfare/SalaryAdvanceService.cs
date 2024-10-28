using System.Net.Http.Json;
using Shared.Helpers;
using Shared.Models.Welfare;

namespace Client.Services.Welfare;


public interface ISalaryAdvanceService
{
    Task<bool> AddAsync(SalaryAdvance service);
    Task<bool> EditAsync(SalaryAdvance service);
    Task<bool> DeleteAsync(Guid id);
    Task<SalaryAdvance?> GetAsync(Guid id);
    Task<SalaryAdvance[]?> GetAllAsync();
    Task<GridDataResponse<SalaryAdvance>?> GetPaged(PaginationParameter parameter);
    Task<GridDataResponse<WelfareData>?> GetPagedData(PaginationParameter parameter);
}
public class SalaryAdvanceService(IHttpClientFactory _client) : ISalaryAdvanceService, IDisposable
{
    public async Task<bool> AddAsync(SalaryAdvance service)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/SalaryAdvance", service);
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
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/SalaryAdvance/{id}");
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

    public async Task<bool> EditAsync(SalaryAdvance service)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/SalaryAdvance/{service.Id}", service);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<SalaryAdvance[]?> GetAllAsync()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<SalaryAdvance[]?>("api/SalaryAdvance");            
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<SalaryAdvance?> GetAsync(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<SalaryAdvance?>($"api/SalaryAdvance/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<GridDataResponse<SalaryAdvance>?> GetPaged(PaginationParameter parameter)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/SalaryAdvance/paged", parameter);            
            return await response.Content.ReadFromJsonAsync<GridDataResponse<SalaryAdvance>?>();
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
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/SalaryAdvance/pagedprojection", parameter);            
            return await response.Content.ReadFromJsonAsync<GridDataResponse<WelfareData>?>();
        }
        catch (Exception)
        {

            throw;
        }    
    }
}