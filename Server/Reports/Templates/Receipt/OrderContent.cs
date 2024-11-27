using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shared.Models.Orders;

namespace Server.Pages.Reports.Templates.Receipt
{
    public class OrderContent(Order Order) : IComponent
    {
        public void Compose(IContainer container)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Item().Row(row =>
                {                    
                    row.RelativeItem().AlignRight().Text($"#: {Order!.ReceiptNo}").FontSize(8);
                });
                column.Item().Row(row =>
                {                    
                    row.RelativeItem().AlignRight().Text($"Date: {Order!.OrderDate.ToString("dd/MM/yyyy")}").FontSize(8);
                });
                column.Spacing(8);
            });
        }
    }
}
