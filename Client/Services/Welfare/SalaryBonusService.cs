using System.Net.Http.Json;
using Shared.Helpers;
using Shared.Models.Welfare;

namespace Client.Services.Welfare;


public interface ISalaryBonusService
{
    Task<bool> AddAsync(SalaryBonus service);
    Task<bool> EditAsync(SalaryBonus service);
    Task<bool> DeleteAsync(Guid id);
    Task<SalaryBonus?> GetAsync(Guid id);
    Task<SalaryBonus[]?> GetAllAsync();
    Task<GridDataResponse<SalaryBonus>?> GetPaged(PaginationParameter parameter);
    Task<GridDataResponse<WelfareData>?> GetPagedData(PaginationParameter parameter);
}
public class SalaryBonusService(IHttpClientFactory _client) : ISalaryBonusService, IDisposable
{
    public async Task<bool> AddAsync(SalaryBonus service)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/salarybonus", service);
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
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/salarybonus/{id}");
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

    public async Task<bool> EditAsync(SalaryBonus service)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/salarybonus/{service.Id}", service);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<SalaryBonus[]?> GetAllAsync()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<SalaryBonus[]?>("api/salarybonus");            
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<SalaryBonus?> GetAsync(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<SalaryBonus?>($"api/salarybonus/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<GridDataResponse<SalaryBonus>?> GetPaged(PaginationParameter parameter)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/salarybonus/paged", parameter);            
            return await response.Content.ReadFromJsonAsync<GridDataResponse<SalaryBonus>?>();
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
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/salarybonus/pagedprojection", parameter);            
            return await response.Content.ReadFromJsonAsync<GridDataResponse<WelfareData>?>();
        }
        catch (Exception)
        {

            throw;
        }    
    }
}