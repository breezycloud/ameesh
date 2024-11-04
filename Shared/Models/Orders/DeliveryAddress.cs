namespace Shared.Models.Orders;
public class DeliveryAddress
{
    public string? State { get; set; }
    public string? Lga { get; set; }
    public string? Ward { get; set; }
    public string? Address { get; set; }
    public DateOnly? DispatchedDate { get; set; }
    public DateOnly? DeliveryDate { get; set; }

}