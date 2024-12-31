using ApexCharts;
using Microsoft.JSInterop;
using Shared.Helpers;
using Shared.Models.Orders;
using Shared.Models.Products;
using Shared.Models.Reports;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Client.Services.Orders;

public interface IOrderService
{
    Task<bool> AddProductOrder(Order model);
    Task<bool> UpdateOrder(Order? model);
    Task<bool> CompleteOrder(Order? model);
    Task<bool> CompleteOrder(Guid id);
    Task DispatchOrder(Guid id);
    Task DeliveredOrder(Guid id);
    Task<bool> UpdateOrderItems(List<ProductOrderItem> items);
    Task<bool> PutPayment(Payment model);
    Task<bool> PutPayments(Payment[] payments);
    Task<bool> DeletePayment(Guid id);
    Task<int> GetReceiptNo(string Type, Guid StoreID);
    string GenerateReceiptNo();
    Task<bool> ValidateReceiptNo(string Type, string ReceiptNo);
    Task<bool> ValidateOrderID(string Type, Guid id);
    Task<bool> ValidateItemQty(Guid id, int qty);
    Task<Order?> GetOrder(Guid id);
    Task<Order?> GetOrder(string? receiptNumber, Guid storeId);
    List<ProductOrderItem> OrderItems(Guid id, List<OrderCartRow> rows);
    List<ThirdPartyItem> ThirdPartyItems(List<OrderCartRow> rows);
    Task<GridDataResponse<OrderWithData>?> GetOrderByStore(string Type, PaginationParameter parameter);
    Task<GridDataResponse<OrderWithData>?> GetOrderWithReturns(PaginationParameter parameter);
    ICollection<OrderReferer> GetOrderReferers(Guid id, string type, ICollection<OrderReferer> referers);
    Task<ReportData?> GetReceipt(string Type, ReportData report);
    Task GetReceiptBase64String(string Type, ReportData report);
    Task PrintReceipt(string Type, ReportData report);
    Task GetBillQrCode(Guid id, string Type, string ReceiptNo);
    Task PrintBill(string ReceiptNo);
    Task<(Guid storeId, byte[]? buffer)> PrintDocument(Guid storeId, Guid id, string Type, string ReceiptNo);
    Task SaleReport(ReportFilter filter);
    Task CancelOrder(Guid id);
    Task<bool> GetPaymentStatus(Guid id);
    Task<bool> Delete(Guid id);
}
public class OrderService : IOrderService
{
    private readonly IHttpClientFactory _client;
    private readonly IJSRuntime _js;

    byte[]? content = null!;
    public OrderService(IHttpClientFactory client, IJSRuntime js)
    {
        _client = client;
        _js = js;
    }
    public async Task<bool> Delete(Guid id)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").DeleteAsync($"api/orders/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }
    public List<ProductOrderItem> OrderItems(Guid id, List<OrderCartRow> rows)
    {
        var items = rows.AsParallel().Where(x => !x.IsThirdParty).Select(i => new ProductOrderItem
        {
            OrderId = id,
            ProductId = i!.Product!.Id,
            StockId = i!.Stock!.id,
            BuyPrice = i!.Stock!.BuyPrice!.Value,
            Product = i!.Product!.ProductName,
            Cost = i!.Cost,
            Quantity = i!.Quantity,
        }).ToList();
        return items;
    }

    public List<ThirdPartyItem> ThirdPartyItems(List<OrderCartRow> rows)
    {
        var items = rows.AsParallel().Where(x => x.IsThirdParty).Select(i => new ThirdPartyItem
        {
            Id = i!.ThirdPartyItem!.Id,
            ItemName = i!.ThirdPartyItem!.ItemName,
            Cost = i!.ThirdPartyItem!.Cost,
            Price = i!.ThirdPartyItem!.Price!,
            Quantity = i!.ThirdPartyItem!.Quantity,
        }).ToList();
        return items;
    }

    public async Task CancelOrder(Guid id)
    {
        try
        {
            await _client.CreateClient("AppUrl").PostAsJsonAsync("api/orders/cancelorders", id);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task DispatchOrder(Guid id)
    {
        try
        {
            await _client.CreateClient("AppUrl").PostAsJsonAsync("api/orders/dispatch", new CompleteBill(id, Guid.Empty));
        }
        catch (Exception)
        {

            throw;
        }
    }

     public async Task DeliveredOrder(Guid id)
     {
      try
        {
            await _client.CreateClient("AppUrl").PostAsJsonAsync("api/orders/delivered", new CompleteBill(id, Guid.Empty));
        }
        catch (Exception)
        {

            throw;
        }  
     }
    public async Task<bool> UpdateOrder(Order? model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/orders/{model!.Id}", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> CompleteOrder(Order? model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync($"api/orders/complete", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> CompleteOrder(Guid id)
    {
        try
        {

            var request = _client.CreateClient("AppUrl").PostAsJsonAsync($"api/orders/completeorder", new CompleteBill(id, Guid.Empty));
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> UpdateOrderItems(List<ProductOrderItem> items)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/orderitems/bulkupdate", items);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> PutPayment(Payment model)
    {
        try
        {
            model.Cashier = null;
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/payments/{model!.Id}", model);
            var response = await request;
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> PutPayments(Payment[] payments)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PutAsJsonAsync($"api/payments/order", payments);
            var response = await request;
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> DeletePayment(Guid id)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").DeleteAsync($"api/payments/{id}");
            var response = await request;
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> AddProductOrder(Order model)
    {
        try
        {
            var request = _client.CreateClient("AppUrl").PostAsJsonAsync("api/orders", model);
            var response = await request;
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {

            throw;
        }
    }


    public async Task<GridDataResponse<OrderWithData>?> GetOrderByStore(string Type, PaginationParameter parameter)
    {
        HttpResponseMessage? response = null;
        try
        {
            response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/orders/paged", parameter);
            return await response.Content.ReadFromJsonAsync<GridDataResponse<OrderWithData>>();
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<GridDataResponse<OrderWithData>?> GetOrderWithReturns(PaginationParameter parameter)
    {
        try
        {
            var response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/orders/withreturns/paged", parameter);
            return await response.Content.ReadFromJsonAsync<GridDataResponse<OrderWithData>>();
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<int> GetReceiptNo(string Type, Guid StoreID)
    {
        int receiptNbr = await _client.CreateClient("AppUrl").GetFromJsonAsync<int>($"api/orders/receiptno/{StoreID}");
        return receiptNbr;
    }

    public string GenerateReceiptNo()
    {
        long ticks = DateTime.UtcNow.Ticks;
        var now = DateTime.UtcNow;
        string uniqueid = Guid.NewGuid().ToString().Substring(0, 8);
        var receiptNo = $"{now:HHmmss}";
        var number = receiptNo.Insert(receiptNo.Length - 2, uniqueid);
        return number;
    }

    public async Task<bool> ValidateReceiptNo(string Type, string ReceiptNo)
    {
        bool response = false;
        response = await _client.CreateClient("AppUrl").GetFromJsonAsync<bool>($"api/orders/validatereceiptno/{ReceiptNo}");
        return response;
    }

    public async Task<bool> ValidateOrderID(string Type, Guid id)
    {
        bool response = false;
        response = await _client.CreateClient("AppUrl").GetFromJsonAsync<bool>($"api/orders/validateorderid/{id}");
        return response;
    }

    public async Task<bool> ValidateItemQty(Guid id, int qty)
    {
        bool response = false;
        response = await _client.CreateClient("AppUrl").GetFromJsonAsync<bool>($"api/products/validateqty?id={id}&qty={qty}");
        return response;
    }
    public async Task<(Guid storeId, byte[]? buffer)> PrintDocument(Guid storeID, Guid id, string Type, string ReceiptNo)
    {
        await GetBillQrCode(id, Type, ReceiptNo);
        if (content is not null)
        {
            return await Task.FromResult((storeID, content));
        }
        return (storeID, null);
    }
    public async Task GetBillQrCode(Guid id, string Type, string ReceiptNo)
    {        
        await _js.InvokeVoidAsync("XPrinter.Test", Convert.ToBase64String(content));
    }
    public async Task GetReceiptBase64String(string Type, ReportData report)
    {
        try
        {            
            await _js.InvokeVoidAsync("XPrinter.Test", Convert.ToBase64String(content));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    private byte[] ConvertToBytes(IEnumerable<byte[]> bytes)
    {
        List<byte> resultList = [];
        foreach (var byteArray in bytes)
        {
            resultList.AddRange(byteArray);
        }
        return resultList.ToArray();
    }
    public async Task<ReportData?> GetReceipt(string Type, ReportData report)
    {
        byte[]? content = null;
        try
        {
            // string OrderID = string.Empty;
            // OrderID = report.Order!.ReceiptNo!;

            // report.ReportHeader = new ReportHeader
            // {
            //     Store = report.Branch,
            //     Logo = await _client.CreateClient("AppUrl").GetByteArrayAsync("icon-512.png"),
            //     Title = report.ReportType
            // };
            // report.ReportFooter = new ReportFooter();
            // report.ReportFooter.QR = new Converter().ConvertToByte(OrderID);
            // var receipt = new ReceiptTemplate(report);
            // content = await receipt.Create();
            if (content != null)
            {
                await _js.InvokeAsync<object>("exportFile", $"Receipt-{Guid.NewGuid()}.pdf", Convert.ToBase64String(content));
                return report;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            await _js.InvokeVoidAsync("console.log", report);
        }
        return null!;
    }
    public ICollection<OrderReferer> GetOrderReferers(Guid id, string type, ICollection<OrderReferer> referers)
    {
        if (!referers.Any())
            return new List<OrderReferer>();

        foreach (var item in referers)
        {
            item.OrderId = id;
        }
        _js.InvokeVoidAsync("console.log", referers);
        return referers;
    }
    public async Task<Order?> GetOrder(Guid id)
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<Order?>($"api/orders/{id}");
    }
    public async Task<Order?> GetOrder(string? receiptNumber, Guid storeId)
    {
        return await _client.CreateClient("AppUrl").GetFromJsonAsync<Order?>($"api/orders/byreceiptno?rno={receiptNumber}&storeId={storeId}");
    }
    public async Task SaleReport(ReportFilter filter)
    {
        string reportName = $"{filter!.ReportOption} Report {(filter.Criteria == "Date" ? $"{filter.StartDate:d}" : $"{filter.StartDate:d} - {filter.EndDate:d}")}.pdf";
        SalesReportTemplate? template = null;

        HttpResponseMessage response = await _client.CreateClient("AppUrl").PostAsJsonAsync("api/orders/report", filter);

        var content = await response.Content.ReadAsByteArrayAsync();
        try
        {                
            await _js.InvokeAsync<object>("exportFile", reportName, Convert.ToBase64String(content));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    public async Task PrintBill(string ReceiptNo)
    {
        await _js.InvokeVoidAsync("XPrinter.PrintBill", ReceiptNo);
    }
    public async Task PrintReceipt(string Type, ReportData report)
    {
        await _js.InvokeVoidAsync("XPrinter.Print", Type, report);
    }
    public async Task<bool> GetPaymentStatus(Guid id)
    {
        try
        {
            return await _client.CreateClient("AppUrl").GetFromJsonAsync<bool>($"api/orders/paymentstatus/{id}");
        }
        catch
        {
            return false;
        }
    }
}
