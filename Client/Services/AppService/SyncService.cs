using System.Net.Http.Json;
using Shared.Helpers;

interface ISyncService
{
    Task Sync();
}

public class SyncService : ISyncService
{
    private readonly IHttpClientFactory _client;

    public SyncService(IHttpClientFactory client)
    {
        _client = client;
    }
    public async Task Sync()
    {
        try
        {
            using var request = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/sync/push", new BackupFilter());            
            request.EnsureSuccessStatusCode();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
        }        
    }
}