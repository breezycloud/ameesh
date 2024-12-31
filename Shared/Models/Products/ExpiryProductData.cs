using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shared.Models.Products;

public class ExpiryProductData
{
    public Guid ProductId {  get; set; }
    public Guid StoreId {  get; set; }
    public Guid StockId { get; set; }
    public DateTime Date { get; set; }
    public string? ProductName { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public decimal Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int DaysToExpire => ExpiryDate!.Date.Subtract(DateTime.UtcNow.Date).Days;
}
