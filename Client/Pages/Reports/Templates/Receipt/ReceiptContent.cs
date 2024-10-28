using MudBlazor;
using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shared.Models.Orders;
using Shared.Models.Reports;
using Client.Layout.AppUI;

namespace Client.Pages.Reports.Templates.Receipt
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
                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text($"Date: {Model!.Order!.OrderDate}").FontSize(8);
                });
                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text($"Name: {(Model!.Customer!.Regular ? "Walk-In" : Model!.Customer!.CustomerName)}").FontSize(8);
                });
                Details = Model!.Order!.ProductOrders.GroupBy(x => x.ProductId, (x, y) => new OrderItemDetail
                {
                    Quantity = y.Select(z => z.Quantity).First(),
                    ItemName = y.Select(c => c.Product).FirstOrDefault(),
                    Cost = y.Select(z => z.Cost).First()
                }).ToList();
                Discount = Model.Order!.Discount;
                Balance = Model!.Order!.Balance;
                AmountPaid = Model!.Order!.Payments.OrderByDescending(x => x.PaymentDate).Select(x => x.Amount).First();             
                column.Spacing(8);
                column.Item().Component(new ReceiptTable(Model.ReportType!, Discount, AmountPaid, Balance, Details));
            });
        }
    }
}
