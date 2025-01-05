using MudBlazor;
using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shared.Models.Orders;
using Shared.Models.Reports;
using Client.Layout.AppUI;
using Client.Handlers;

namespace Server.Pages.Reports.Templates.Receipt
{
    
    public class ReportContent(ReportData Model) : IComponent
    {
        List<OrderItemDetail> Details = new();
        public void Compose(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                decimal Discount, AmountPaid, Balance = 0m;
                var Order = Model!.Order!;
                if (Model?.Order?.ProductOrders != null)
                {
                    Details = Model.Order.ProductOrders.GroupBy(x => x.ProductId, (x, y) => new OrderItemDetail
                    {
                        Quantity = y.Select(z => z?.Quantity)?.First(),
                        ItemName = y.Select(c => c?.Product)?.FirstOrDefault(),
                        Cost = y.Select(z => z.Cost).First()
                    }).ToList();
                }
                if (Model?.Order?.ThirdPartyItems != null)
                {
                    var orderItemDetails = Model.Order.ThirdPartyItems.Select(x => new OrderItemDetail
                    {
                        Quantity = x.Quantity,
                        ItemName = x.ItemName,
                        Cost = x.Price
                    }).ToList();
                    Details.AddRange(orderItemDetails);
                }
                Discount = Model!.Order!.Discount;
                Balance = Model!.Order!.Balance;
                AmountPaid = Model!.Order!.Payments.OrderByDescending(x => x.PaymentDate).Select(x => x.Amount).First();
                column.Spacing(8);
                column.Item().Component(new ReceiptTable(Model.ReportType!, Discount, AmountPaid, Balance, Details, Model!.Order));
            });
        }
    }
}
