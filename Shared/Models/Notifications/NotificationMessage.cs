using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models.Notifications;
public record NotificationMessage
{
    [Key]
    public Guid id { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Category { get; set; }
    public DateTime PublishDate { get; set; }    
    public bool IsRead { get; set; }
}
