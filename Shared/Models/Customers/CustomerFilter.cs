using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Customers;
public class CustomerFilter
{    
    public CustomerFilterType FilterType { get; set; } = CustomerFilterType.All;
    [Required(ErrorMessage = "Filter is required!")]
    public string? Type => FilterType.ToString();
}
public enum CustomerFilterType
{
    All,
    WithAddress,
    WithoutAddress,
}