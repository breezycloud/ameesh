using System.Net.Http.Json;
using Shared.Models.Products;

namespace Client.Services.Returns;


public interface IReturnService
{
    Task<bool> Put(RefundPayload payload);
    Task<bool> Delete(Guid id);
}

public class ReturnService(IHttpClientFactory _client) : IReturnService
{
    public async Task<bool> Delete(Guid id)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").DeleteAsync($"api/returns/{id}");
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> Put(RefundPayload payload)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PutAsJsonAsync($"api/returns/{payload!.Product!.Id}", payload);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }
}
