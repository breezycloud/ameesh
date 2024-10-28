namespace Shared.Helpers;

public class BackupFilter
{
    public string? Option {get; set; } = "Backup";
    public DateTime? LastUpdate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
}