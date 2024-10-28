using Shared.Models.Auth;
using System.Net.Http.Json;

namespace Client.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse?> Login(LoginModel model);
}

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _client;

    public AuthService(IHttpClientFactory client)
    {
        _client = client;
    }

    public async Task<LoginResponse?> Login(LoginModel model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/users/login", model);
            var response = await request;
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
        catch (Exception)
        {

            throw;
        }
        
    }
}
