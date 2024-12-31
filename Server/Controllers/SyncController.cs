using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Server.Hubs;
using Shared.Helpers;
using Shared.Models.Products;
using System.Data;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{        
    private readonly ILogger<SyncController> _logger;
    private readonly IHubContext<SignalRHubs> _hub;
    private readonly IConfiguration _config;
    public SyncController(ILogger<SyncController> logger, IHubContext<SignalRHubs> hub, IConfiguration config)
    {
        _logger = logger;
        _hub = hub;
        _config = config;
    }
    
    [HttpPost("push")]
    public async Task<ActionResult> Post(BackupFilter filter) 
    {
        _logger.LogInformation("Synchronization Started at {0}", DateTime.UtcNow.TimeOfDay);
        double percentage = 0;
        try
        {
            
            //var tables = new[] {"\"Charges\"","\"Stores\"","\"Customers\"","\"Expenses\"","\"ExpenseTypes\"","\"Logs\"","\"Audits\""};
            // Original array
            var tables = new[] { 
                "Users", "PharmacyServices", "Brands", "Categories", "Items", 
                "Products", "Charges", "Stores", "Customers", "ExpenseTypes", 
                "Expenses", "Logs", "Audits", "Orders", "OrderItems", 
                "OtherService", "OrderReferers", "Payments", "ReturnedProducts", "Referers" 
            };

            // Create a new array of formatted table names
            var formattedTables = tables
                .Select((table, index) => new { TableName = table, Index = index })
                .OrderBy(item => item.Index) // Order by index, though it will be naturally ordered
                .Select(item => $"\"{item.TableName}\"")
                .ToArray();                                                                                                                                                                                                                                       
            
            using (var sourceConn = new NpgsqlConnection(_config.GetConnectionString("Local")))
            using (var targetConn = new NpgsqlConnection(_config.GetConnectionString("Server")))
            {
                await sourceConn.OpenAsync();
                await targetConn.OpenAsync();

                // GET Last synchronized
                var date = GetLastUpdated(sourceConn);
                if (date is not null)
                    filter.LastUpdate = date;
                
                _logger.LogInformation("Last updated: {0}", filter!.LastUpdate.GetValueOrDefault());                
                
                int steps =  formattedTables.Length;            
                for (int i = 0; i < steps; i++)
                {
                    var table = formattedTables[i];
                    percentage = i * 100 / steps;
                    var progress = new SyncProgress(percentage);
                    _logger.LogInformation("{1} Synchronization Progress: {0}", progress, table);                

                    // Retrieve data from source                                        
                    var sourceData = GetDataFromDatabase(sourceConn, table, filter.LastUpdate.GetValueOrDefault());

                    // Perform upsert for each row
                    foreach (DataRow sourceRow in sourceData.Rows)
                    {
                        await UpsertRow(targetConn, table, sourceRow);
                    }
                    
                    await _hub.Clients.All.SendAsync("SyncProgress", progress);
                }
                steps++;
                percentage = steps * 100 / steps;
                await _hub.Clients.All.SendAsync("SyncProgress", new SyncProgress(percentage, "Synchronization completed"));
                await UpdateLastUpdate(sourceConn, targetConn);
            }
        }
        catch (System.Exception ex)
        {        
            _logger.LogError(ex.Message);
            await _hub.Clients.All.SendAsync("SyncProgress", new SyncProgress(percentage, "Synchronization failed"));
            return BadRequest(ex.Message);
        }        
        _logger.LogInformation("Synchronization Stopped at {0}", DateTime.UtcNow.TimeOfDay);
        return Ok();
    }

    private DateTime? GetLastUpdated(NpgsqlConnection sourceConn)
    {
        var field = "LastUpdate";
        var table = "Backups";
        DataTable data = new();
        using (var cmd = new NpgsqlCommand($"SELECT * FROM {"\"" +  table + "\""} ORDER BY {"\"" +  field + "\""} DESC", sourceConn))
        using (var row = new NpgsqlDataAdapter(cmd))
        {
            row.Fill(data);
            if (data.Columns.Count > 0)
            {
                data.PrimaryKey = [data.Columns[0]];
            }
        }
        
        if (data.Rows.Count > 0)
            return data.Rows[0].Field<DateTime>(1);
        
        return null;
    }

    private async Task UpdateLastUpdate(NpgsqlConnection sourceConn, NpgsqlConnection targetConn)
    {
        _logger.LogInformation("Updating last backup timestamp at {0}", DateTime.UtcNow.TimeOfDay);
        var field = "\"LastUpdate\"";
        var tableName = "\"Backups\""; // No need to quote table names in this context

        var lastModified = DateTime.UtcNow;
        // Use parameterized queries
        var sql = $@"
            INSERT INTO {tableName} ({field}) VALUES (@LastUpdated)";
        

        // Update source
        _logger.LogInformation("Updating source backup timestamp at {0}", DateTime.UtcNow.TimeOfDay);
        using var sourceCmd = new NpgsqlCommand(sql, sourceConn);
        sourceCmd.Parameters.AddWithValue($"@LastUpdated", NpgsqlTypes.NpgsqlDbType.Timestamp, lastModified);
        await sourceCmd.ExecuteNonQueryAsync();

        // Update target
        _logger.LogInformation("Updating target backup timestamp at {0}", DateTime.UtcNow.TimeOfDay);
        using var targetCmd = new NpgsqlCommand(sql, targetConn);
        targetCmd.Parameters.AddWithValue("@LastUpdated", NpgsqlTypes.NpgsqlDbType.Timestamp, lastModified);
        await targetCmd.ExecuteNonQueryAsync();

    }    

    private string[]? TablesWithCreatedAtOnly = ["Logs", "Audits"];
    private string[]? TablesWithCreatedDateOnly = ["Payments"];
    private string[]? TablesNoDate = ["OrderItems", "OtherService"];
    private string[]? TablesWithDate = ["ReturnedProducts"];
    int totalChanges = 0;
    private DataTable GetDataFromDatabase(NpgsqlConnection conn, string tableName, DateTime recent)
    {
        string? field = string.Empty;
        string? field2 = string.Empty;
        bool doubleParams = false;
        string? query = string.Empty;
        var dataTable = new DataTable();
        var trimmedQuotes = tableName.Remove(0, 1);
        var count = trimmedQuotes.Length - 1;
        trimmedQuotes = trimmedQuotes.Remove(count,1);        
        if (TablesWithCreatedAtOnly!.Contains(trimmedQuotes))
        {
            field = "CreatedAt";
            query = $"SELECT * FROM {tableName} WHERE {"\"" +  field + "\""} >= @{field}";
        }                    
        else if (TablesWithCreatedDateOnly!.Contains(trimmedQuotes))
        {
            field = "CreatedDate";
            query = $"SELECT * FROM {tableName} WHERE {"\"" +  field + "\""} >= @{field}";
        }   
        else if (TablesNoDate!.Contains(trimmedQuotes))        
        {
            field = "ModifiedDate";
            query = $"SELECT * FROM {tableName} WHERE {"\"" +  field + "\""} >= @{field}";
        }            
        else if (TablesWithDate!.Contains(trimmedQuotes))        
        {
            field = "Date";
            query = $"SELECT * FROM {tableName} WHERE {"\"" +  field + "\""} >= @{field}";                 
        }
        else
        {
            field = "ModifiedDate";
            field2 = "CreatedDate";
            query = $"SELECT * FROM {tableName} WHERE {"\"" +  field + "\""} >= @{field} OR {"\"" +  field2 + "\""} >= @{field}";
            doubleParams = true;
        }
        using (var cmd = new NpgsqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue($"@{field}", NpgsqlTypes.NpgsqlDbType.Timestamp, recent);
            using (var adapter = new NpgsqlDataAdapter(cmd))
            {            
                adapter.Fill(dataTable);
                if (dataTable.Columns.Count > 0)
                {
                    dataTable.PrimaryKey = [dataTable.Columns[0]];
                    totalChanges = dataTable.Rows.Count;                    
                }
            }
        }                
        return dataTable;
    }
    private async Task UpsertRow(NpgsqlConnection conn, string tableName, DataRow row)
    {
        int index = 0;
        try
        {
            var columns = string.Join(", ", row.Table.Columns.Cast<DataColumn>().Select(c => "\"" + c.ColumnName + "\""));
            
            var values = string.Join(", ", row.Table.Columns.Cast<DataColumn>().Select(c => $"@{c.ColumnName}"));
                
            var updateSet = string.Join(", ", row.Table.Columns.Cast<DataColumn>()
                .Where(c => c.Ordinal != 0)
                .Select(c => $"\"{c.ColumnName}\" = EXCLUDED.\"{c.ColumnName}\""));


            var source = string.Join(", ", row.Table.Columns.Cast<DataColumn>()
                .Where(c => c.Ordinal != 0) // Exclude the first (primary key) column
                .Select(c => $"src.\"{c.ColumnName}\""));
            

            var primaryKeyColumn = "\"" +row.Table.Columns[0].ColumnName + "\""; // Assuming first column is primary key
            
            // var sql = $@"
            //     INSERT INTO public.{tableName} ({columns})
            //     SELECT {source} FROM {tableName} AS src
            //     ON CONFLICT ({primaryKeyColumn}) DO UPDATE
            //     SET {updateSet}";

            var sql = $@"
                INSERT INTO {tableName} ({columns})
                VALUES ({values})
                ON CONFLICT ({primaryKeyColumn}) DO UPDATE
                SET {updateSet}";

            
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 0;
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    if (row.Table.Columns[i].ColumnName == "Dispensary" || row.Table.Columns[i].ColumnName == "Stocks") // Check if it's the "Dispensary" column                    
                        cmd.Parameters.AddWithValue($"@{row.Table.Columns[i].ColumnName}", NpgsqlTypes.NpgsqlDbType.Jsonb, row[i]!);
                    else
                        cmd.Parameters.AddWithValue($"@{row.Table.Columns[i].ColumnName}", row[i] ?? DBNull.Value);
                }                
                await cmd.ExecuteNonQueryAsync();
                index++;
                Console.WriteLine("{0} Updated row {1} of {2}", tableName, index, totalChanges);
            }
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }
}