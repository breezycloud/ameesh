namespace Shared.Models.Products;


public class RefundPayload
{
    public Guid CustomerId { get; set; }
    public bool CType { get; set; }
    public ReturnedProduct? Product { get; set; }
}