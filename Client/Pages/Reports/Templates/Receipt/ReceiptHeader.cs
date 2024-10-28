using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shared.Models.Reports;

namespace Client.Pages.Reports.Templates.Receipt
{
    public class HeaderContent(ReportHeader header) : IComponent
    {
        private int pictureSize = 25;
        public void Compose(IContainer container)
        {
            container.AlignCenter().Column(column =>
            {
                column.Item()
                      .AlignCenter()
                      .Text($"Ameesh Luxury")
                      .Bold()
                      .FontSize(10);

                column.Item()
                      .AlignCenter()
                      .Text($"{header!.Store!.BranchAddress}")
                      .FontSize(8);

                column.Item()
                      .AlignCenter()
                      .Text($"{header!.Store.PhoneNo1}")
                      .FontSize(8);
                
                column.Item()
                      .AlignCenter()
                      .Text($"{header!.Title}")
                      .FontSize(8);
                //column.Item().AlignCenter().Text($"Receipt {Model!.Order!.ReceiptNo.ToString().PadLeft(4, '0')}").FontSize(8);
            });
        }
    }
}
