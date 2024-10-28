using Shared.Helpers;
using Shared.Models.Customers;
using Shared.Models.Locations;
using System.Net.Http.Json;
using System.Reflection;
using Blazored.LocalStorage;

namespace Client.Services.Customers;

public interface ICustomerService
{
    Task<bool> AddCustomer(Customer model);
    Task<bool> EditCustomer(Customer model);
    Task<bool> DeleteCustomer(Guid id);
    Task<Customer?> GetCustomerById(Guid id);
    Task<Customer?> GetCustomerByPhone(string phone);
    Task<Customer[]?> GetCustomers();
    Task<GridDataResponse<Customer>> GetPagedCustomers(PaginationParameter parameter);
    Task<GridDataResponse<CustomerData>> GetPagedCustomerData(PaginationParameter parameter);
    Task<StateLgaWard[]?> GetLocations();
}
public class CustomerService(IHttpClientFactory _client, ILocalStorageService localStorage) : ICustomerService
{

    public async Task<bool> AddCustomer(Customer model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/customers", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> DeleteCustomer(Guid id)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/customers/{id}");
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> EditCustomer(Customer model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/customers/{model.Id}", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Customer?> GetCustomerById(Guid id)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").GetFromJsonAsync<Customer?>($"api/customers/{id}");
            var response = await request;
            return response;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Customer?> GetCustomerByPhone(string phone)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").GetFromJsonAsync<Customer?>($"api/customers/byPhone/{phone}");
            var response = await request;
            return response;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Customer[]?> GetCustomers()
    {
        try
        {
            var request = _client.CreateClient("AppUrl").GetFromJsonAsync<Customer[]?>($"api/customers");
            var response = await request;            
            return response;
        }
        catch (Exception)
        {

            throw;
        }
    }

	public async Task<GridDataResponse<Customer>> GetPagedCustomers(PaginationParameter parameter)
	{
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/customers/paged", parameter);
            var response = await request;
            var contents= await response.Content.ReadFromJsonAsync<GridDataResponse<Customer>>();
            return contents!;
        }
        catch (Exception)
        {

            throw;
        }
	}

    public async Task<GridDataResponse<CustomerData>> GetPagedCustomerData(PaginationParameter parameter)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/customers/paged", parameter);
            var response = await request;
            var contents = await response.Content.ReadFromJsonAsync<GridDataResponse<CustomerData>>();
            return contents!;
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<StateLgaWard[]?> GetLocations()
    {
        StateLgaWard[]? locations = null;
        try
        {
            Console.WriteLine("Getting local...");
            locations = await localStorage.GetItemAsync<StateLgaWard[]?>("locations") ?? null;
            if (locations is null)
            {
                Console.WriteLine("Getting api");
                locations = await _client.CreateClient("AppUrl").GetFromJsonAsync<StateLgaWard[]?>("locations.json");
                await localStorage.SetItemAsync("locations", locations);
            }
            return locations;

        }
        catch (System.Exception)
        {
            
            throw;
        }
    }
}
