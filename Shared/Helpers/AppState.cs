using Shared.Helpers;
using Shared.Models.Reports;
using Shared.Models.Products;
using Shared.Models.Locations;
using Shared.Models.Orders;
using Shared.Models.Welfare;
using Shared.Enums;

namespace Shared.Models;
public class AppState
{
    public DateTime CurrentDateTime { get; set; } = DateTime.Now;
    public event EventHandler? OnUpdateLayout;
    public void UpdateLayout() => OnUpdateLayout?.Invoke(this, EventArgs.Empty);
    public event EventHandler? OnCheckOut;
    public void CheckOut() => OnCheckOut?.Invoke(this, EventArgs.Empty);

    
    public event EventHandler<bool>? RefererHandler;
    public void RefererSelected(bool value) => RefererHandler?.Invoke(this, value);

    public event EventHandler<int>? OnUpdateItemQty;
    public void UpdateItemQuantity(int value) => OnUpdateItemQty?.Invoke(this, value);
    public event EventHandler<CartRowUpdate>? OnRowUpdate;
    public void UpdateItemQuantity(CartRowUpdate value) => OnRowUpdate?.Invoke(this, value);

    public event EventHandler<Payment>? OnPayment;
    public void UpdatePayment(Payment payment) => OnPayment?.Invoke(this, payment);

    public event EventHandler<OrderCartUpdateEventArgs>? OnUnhold;
    public void UpdateCart(List<OrderCartRow> bill, int count) => OnUnhold?.Invoke(this, new OrderCartUpdateEventArgs(bill, count));

    public event EventHandler? OnSuccess;
    public void ClearOrderItems() => OnSuccess?.Invoke(this, EventArgs.Empty);
    public WelfareType WelfareType { get; set; } = WelfareType.Salary;
    public string? SelectedOption { get; set; }
    public string? Token { get; set; }
    public string? Role { get; set; }
    public Guid GlobalID { get; set; }
    public Guid StoreID { get; set; }
    public bool IsProcessing { get; set; }
    public bool IsBusy { get; set; }
    public bool IsReadOnly { get; set; }
	public bool Entry { get; set; }
	public bool DialogVisibility { get; set; }
	public string? _searchString {get; set;}
    public bool IsUploading { get; set; }    
    public ReportData? ReportDataModel { get; set; }
    public Product? Product { get; set; }
    public Order? Pharmacy { get; set; }
    public SalaryBonus? SalaryBonus { get; set; }
    public string? Route { get; set; }
    public string GetQrCode() {
        Converter converter = new Converter();
        var code = ReportDataModel!.Order is not null ? ReportDataModel!.Order.Id : ReportDataModel!.Order!.Id;
        return Convert.ToBase64String(converter.ConvertImageToByte(code));
    }
    
    public int DefaultTimer = 10;    

    public static decimal CalculateProfit(IEnumerable<ProductOrderItem> items)
    {
        decimal buy = 0M; 
        decimal sell = 0M;
        foreach (var item in items)
        {
            buy += item.BuyPrice * item.Quantity;
        }
        foreach (var item in items)
        {
            sell += item.Cost * item.Quantity;
        }
    
        return sell - buy;
    
    }

    public static decimal CalculateProfit(IEnumerable<ThirdPartyItem> items)
    {
        decimal buy = 0M; 
        decimal sell = 0M;
        foreach (var item in items)
        {
            buy += item.Cost * item.Quantity;
        }
        foreach (var item in items)
        {
            sell += item.Price * item.Quantity;
        }
    
        return sell - buy;
    
    }

    public static decimal CalculateRefunds(IEnumerable<ReturnedProduct> products, DateOnly p)
    {
        var date = new DateTime(p.Year, p.Month, p.Day).Date;
        return products.Where(r => r.Date.Date == date).Sum(x => x.Cost) ?? 0;
    }

    public StateLgaWard[]? States = [];

    public IEnumerable<string>? SearchState(string value)
    {                
        if (string.IsNullOrWhiteSpace(value))
        {
            return States!.AsParallel().OrderBy(x => x.State).Select(x => x.State!);
        }        
        return States!.AsParallel().OrderBy(x => x.State).Where(x => x.State!.Contains(value, StringComparison.OrdinalIgnoreCase)).Select(x => x.State!);
    }

    public IEnumerable<string>? SearchLga(string state, string value)
    {        
        if (States!.Length == 0 || string.IsNullOrEmpty(state))
            return [];

        if (string.IsNullOrWhiteSpace(value))
        {
            return States!.AsParallel().Where(x => x.State == state).SelectMany(x => x.Lgas!).Select(x => x.Lga!).OrderBy(x => x);
        }        
        return States!.AsParallel().Where(x => x.State == state).SelectMany(x => x.Lgas!).Select(x=> x.Lga!).Where(x => x.Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<string>? SearchWard(string state, string lga, string value)
    {        
        if (States!.Length == 0 || string.IsNullOrEmpty(state) || string.IsNullOrEmpty(lga))
            return [];

        if (string.IsNullOrWhiteSpace(value))
        {
            return States!.AsParallel().Where(x => x.State == state).SelectMany(x => x.Lgas!).Where(x => x.Lga == lga).SelectMany(x => x.Wards!).OrderBy(x => x);
        }        
        return States!.AsParallel().Where(x => x.State == state).SelectMany(x => x.Lgas!).Where(x => x.Lga == lga).SelectMany(x => x.Wards!).Where(x => x.Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    

    public CancellationToken GetCancellationToken()
    {
        CancellationTokenSource source = new CancellationTokenSource();
        return source.Token;
    }
}