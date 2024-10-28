using System.Net.Http.Json;
using Shared.Helpers;
using Shared.Models.Expenses;

namespace Client.Services.Expenses;

public interface IExpenseService
{
    Task<bool> PutExpenseType(ExpenseType type);
    Task<bool> DeleteExpenseType(Guid id);
    Task<ExpenseType?> GetExpenseType(Guid id);
    Task<ExpenseType[]?> GetExpenseTypes();
    Task<GridDataResponse<ExpenseType>?> GetPagedExpenseTypes(PaginationParameter parameter);


    Task<bool> PutExpense(Expense expense);
    Task<bool> DeleteExpense(Guid id);
    Task<Expense?> GetExpense(Guid id);
    Task<Expense[]?> GetExpenses();
    Task<GridDataResponse<Expense>?> GetPagedExpenses(PaginationParameter parameter);
}
public class ExpenseService(IHttpClientFactory client) : IExpenseService
{
    public async Task<bool> DeleteExpense(Guid id)
    {
        try
        {
            var response = await client.CreateClient("AppUrl").DeleteAsync($"api/expenses/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> DeleteExpenseType(Guid id)
    {
        try
        {
            var response = await client.CreateClient("AppUrl").DeleteAsync($"api/expensetypes/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Expense?> GetExpense(Guid id)
    {
        try
        {
            return await client.CreateClient("AppUrl").GetFromJsonAsync<Expense?>($"api/expenses/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Expense[]?> GetExpenses()
    {
        try
        {
            return await client.CreateClient("AppUrl").GetFromJsonAsync<Expense[]?>($"api/expenses");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ExpenseType?> GetExpenseType(Guid id)
    {
        try
        {
            return await client.CreateClient("AppUrl").GetFromJsonAsync<ExpenseType?>($"api/expensetypes/{id}");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ExpenseType[]?> GetExpenseTypes()
    {
        try
        {
            return await client.CreateClient("AppUrl").GetFromJsonAsync<ExpenseType[]?>($"api/expensetypes");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<GridDataResponse<Expense>?> GetPagedExpenses(PaginationParameter parameter)
    {
        try
        {
            var response = await client.CreateClient("AppUrl").PostAsJsonAsync("api/expenses/paged", parameter);
            return await response.Content.ReadFromJsonAsync<GridDataResponse<Expense>>();
        }
        catch (Exception)
        {

            throw;
        }
    }
    public async Task<GridDataResponse<ExpenseType>?> GetPagedExpenseTypes(PaginationParameter parameter)
    {
        try
        {
            var response = await client.CreateClient("AppUrl").PostAsJsonAsync("api/expensetypes/paged", parameter);
            return await response.Content.ReadFromJsonAsync<GridDataResponse<ExpenseType>>();
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> PutExpense(Expense expense)
    {
        try
        {
            var response = await client.CreateClient("AppUrl").PutAsJsonAsync($"api/expenses/{expense.Id}", expense);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> PutExpenseType(ExpenseType type)
    {
        try
        {
            var response = await client.CreateClient("AppUrl").PutAsJsonAsync($"api/expensetypes/{type.Id}", type);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }
}
