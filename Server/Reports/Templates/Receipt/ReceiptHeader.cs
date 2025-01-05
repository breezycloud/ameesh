using Client.Handlers;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shared.Models.Company;
using Shared.Models.Reports;

namespace Server.Pages.Reports.Templates.Receipt
{
    public class HeaderContent(ReportData Model) : IComponent
    {
        private int pictureSize = 25;
        public void Compose(IContainer container)
        {
            container.AlignCenter().Column(column =>
            {
                column.Spacing(5);
                column.Item()
                      .AlignCenter()
                      .Text($"Ameesh Luxury")
                      .Bold()
                      .FontSize(10);

                column.Item()
                      .AlignCenter()
                      .Text($"{Model?.Branch?.BranchAddress}")
                      .FontSize(8);

                column.Item()
                      .AlignCenter()
                      .Text($"{Model?.Branch?.PhoneNo1}")
                      .FontSize(8);
                
                column.Item().Row(row => 
                {
                    row.RelativeItem().Column(col => 
                    {
                        col.Spacing(1);
                        col.Item().AlignLeft().Text($"Billing Details:").FontSize(8).Bold();
                        col.Item().AlignLeft().Text($"Date: {Model!.Order!.OrderDate}").FontSize(8);
                        col.Item().AlignLeft().Text($"Name: {(Model!.Customer!.Regular ? "Walk-In" : Model!.Customer!.CustomerName)}").FontSize(8);
                        col.Item().AlignLeft().Text($"Cashier: {Model!.Cashier}").FontSize(8);
                    });
                    if (Model!.Order!.HasDelievery)
                    {
                        row.RelativeItem().Column(col => 
                        {
                            col.Spacing(1);                    
                            col.Item().AlignLeft().Text($"Delivery Address:").FontSize(8).Bold();
                            col.Item().AlignLeft().Text($"{Model!.Order?.Address?.Address!}, {StringConverter.ConvertToTitleCase(Model!.Order?.Address?.State!)}").FontSize(8);
                        });
                    }      
                });
            });
        }
    }
}
