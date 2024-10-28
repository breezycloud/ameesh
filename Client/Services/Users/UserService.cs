using Shared.Models.Users;
using System.Net.Http.Json;
using System.Reflection;

namespace Client.Services.Users;

public interface IUserService
{
    Task<User?> GetUser(Guid id);
    Task<User?> GetUser(string username);
    Task<bool> AddUser(User model);
    Task<bool> EditUser(User model);
    Task<bool> DeleteUser(Guid id);
    Task<bool> RevokeUserAccount(Guid id);
    Task<User[]?> GetUsers();
    Task<User[]?> GetUsers(CancellationToken token);
    Task<User[]?> GetUsers(Guid storeId);
    Task<User[]?> GetActiveUsers();
    Task<NewPasswordModel?> GetOldPassword(Guid id);
    Task<bool> ChangePassword(Guid id, NewPasswordModel model);
    Task<StaffDto[]?> GetStaffOnly();
}

public class UserService : IUserService
{
    private readonly IHttpClientFactory _client;

    public UserService(IHttpClientFactory client)
    {
        _client = client;
    }

    public async Task<bool> AddUser(User model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/users", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> RevokeUserAccount(Guid id)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/users/revokeuseraccount/{id}");
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public async Task<bool> DeleteUser(Guid id)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/users/{id}");
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> EditUser(User model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/users/{model.Id}", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<User?> GetUser(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<User?>($"api/users/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<User?> GetUser(string username)
    {
        try
        {
            var users = await _client.CreateClient("AppUrl").GetFromJsonAsync<User[]?>($"api/users/username/{username}");
            if (users!.Any())
                return users!.FirstOrDefault();

            return null;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<User[]?> GetUsers()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<User[]?>($"api/users");
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<User[]?> GetUsers(CancellationToken token)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<User[]?>($"api/users", token);
        }
        catch (Exception)
        {

            throw;
        }
    }
    
    public async Task<User[]?> GetActiveUsers()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<User[]?>($"api/users/active");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<User[]?> GetUsers(Guid storeId)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<User[]?>($"api/users/byStore/{storeId}");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<NewPasswordModel?> GetOldPassword(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<NewPasswordModel?>($"api/users/getoldpassword/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<bool> ChangePassword(Guid id, NewPasswordModel model)
    {
        try
        {
            var request = await _client.CreateClient("AppUrl").PostAsJsonAsync($"api/users/changepassword/{id}", model);
            request.EnsureSuccessStatusCode();
            return request.IsSuccessStatusCode;

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<StaffDto[]?> GetStaffOnly()
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<StaffDto[]?>("api/users/staffonly");
        }
        catch (System.Exception)
        {            
            throw;
        }
    }
}
