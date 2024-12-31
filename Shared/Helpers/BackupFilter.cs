namespace Shared.Helpers;

public class BackupFilter
{
    public string? Option {get; set; } = "Backup";
    public DateTime? LastUpdate { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
}