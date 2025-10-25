using System.Net.Http.Json;
using Client.Handlers;
using Microsoft.JSInterop;
using Shared.Helpers;
using Shared.Models.Users;
using Shared.Models.Welfare;

namespace Client.Services.Welfare;


public interface ISalaryService
{
    Task<bool> AddAsync(Salary service);
    Task<bool> EditAsync(Salary service);
    Task<bool> DeleteAsync(Guid id);
    Task<Salary?> GetAsync(Guid id);
    Task<Salary[]?> GetAllAsync();
    Task<StaffDto[]?> GetStaffNotPaid();
    Task<List<ReportCriteria>?> GetSalMonthYear();
    Task<GridDataResponse<Salary>?> GetPaged(PaginationParameter parameter);
    Task<GridDataResponse<WelfareData>?> GetPagedData(PaginationParameter parameter);
    Task GetReport(ReportCriteria criteria);
}
public class SalaryService(IHttpClientFactory _client, IJSRuntime _js) : ISalaryService, IDisposable
{
    public async Task<bool> AddAsync(Salary service)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/Salary", service);
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
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/Salary/{id}");
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

    public async Task<bool> EditAsync(Salary service)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/Salary/{service.Id}", service);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Salary[]?> GetAllAsync()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Salary[]?>("api/Salary");            
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Salary?> GetAsync(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<Salary?>($"api/Salary/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }


    public async Task<List<ReportCriteria>?> GetSalMonthYear()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<List<ReportCriteria>?>("api/salary/salmonthyear");
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task<GridDataResponse<Salary>?> GetPaged(PaginationParameter parameter)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/Salary/paged", parameter);            
            return await response.Content.ReadFromJsonAsync<GridDataResponse<Salary>?>();
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
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/salary/pagedprojection", parameter);            
            return await response.Content.ReadFromJsonAsync<GridDataResponse<WelfareData>?>();
        }
        catch (Exception)
        {

            throw;
        }    
    }

    public async Task<StaffDto[]?> GetStaffNotPaid()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<StaffDto[]?>("api/salary/notpaid");
        }
        catch (System.Exception)
        {            
            throw;
        }
    }

    public async Task GetReport(ReportCriteria criteria)
    {
        byte[]? content = null;

        try
        {
            string endpoint = criteria.ReportType switch
            {
                "Advance" => "api/salaryadvances/report",
                "Bonus" => "api/salarybonus/report",
                "Penalty" => "api/penalties/report",
                _ => "api/salary/report" 
            };

            var client = _client.CreateClient("AppUrl");
            var response = await client.PostAsJsonAsync(endpoint, criteria);

            if (!response.IsSuccessStatusCode)
                return;

            content = await response.Content.ReadAsByteArrayAsync();

            string fileName = $"{StringConverter.ConvertToMonth(criteria.Month!.Value)} {criteria.Year!.Value} {criteria.ReportType} Report.pdf";

            await _js.InvokeVoidAsync(
                "exportFile",
                fileName,
                Convert.ToBase64String(content)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Report generation failed: {ex.Message}");
            throw;
        }
    }

}
