namespace Shared.Models.Products;


public class ThirdPartyItem
{
    public Guid Id { get; set; }
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
}