namespace Shared.Models.Products;

public class ProductCategory
{
    public Guid Id { get; set; }
    public string? ProductName { get; set; }
    public string? CategoryName { get; set; }
    public decimal StocksOnHand { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime ModifiedDate { get; set; }
}