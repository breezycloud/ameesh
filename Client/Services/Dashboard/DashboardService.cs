using Shared.Models.Dashboard;
using System.Net.Http.Json;

namespace Client.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardModel?> GetDashboardDataAsync();
    Task<DashboardModel?> GetDashboardDataAsync(DashboardFilter filter);
}

public class DashboardService : IDashboardService
{
    private readonly IHttpClientFactory _client;

    public DashboardService(IHttpClientFactory client)
    {
        _client = client;
    }

    public async Task<DashboardModel?> GetDashboardDataAsync()
    {
        try
        {
            var request = _client.CreateClient("AppUrl").GetFromJsonAsync<DashboardModel?>("api/dashboard");
            var response = await request;
            return response;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<DashboardModel?> GetDashboardDataAsync(DashboardFilter filter)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/dashboard/filter", filter);
            var response = await request;
            return await response.Content.ReadFromJsonAsync<DashboardModel?>();
        }
        catch (Exception)
        {

            throw;
        }        
    }
}
