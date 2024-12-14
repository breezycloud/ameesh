using Microsoft.AspNetCore.Components.Routing;
using Mud = MudBlazor;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.Reports;
using Shared.Models.Products;

namespace Server.Pages.Reports.Templates.Product;

public class StockAuditReport(UserSoldProduct? Model) : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;
    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4.Portrait());
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
    }

    void ComposeHeader(IContainer container)
    {
        var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

        container.ShowOnce().Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"Ameesh Luxury").Style(titleStyle);

                column.Item().Text(text =>
                {
                    text.Span($"Stock Audit Report as at {Model!.ReportDate.ToLongDateString()}");
                });                
                column.Item().Text(text =>
                {
                    text.Span($"By {Model!.User}");
                });                
            });
            //row.ConstantItem(100).Height(50).Placeholder();
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(5).Column(column =>
        {
            column.Spacing(2);

            column.Item().Element(ComposeTable);            
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Column(column => 
        {
            column.Item().Table(table => {
                // step 1
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(1.2f);            
                    columns.RelativeColumn();                
                    columns.RelativeColumn();                                                
                });

                // step 2
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");                
                    header.Cell().Element(CellStyle).Text("Product Name");
                    header.Cell().Element(CellStyle).AlignCenter().Text("Total Sold Qty");
                    header.Cell().Element(CellStyle).AlignCenter().Text("Current Dispensary Qty");                

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }
                });

                // step 3
                foreach (var item in Model!.Products)
                {
                    table.Cell().Element(CellStyle).Text(text =>
                    {
                        text.Span($"{Model!.Products.IndexOf(item) + 1}").FontSize(9);
                    });
                    table.Cell().Element(CellStyle).Text(item.ProductName).FontSize(9);
                    table.Cell().Element(CellStyle).AlignCenter().Text(item.QtySold.ToString()).FontSize(9);
                    table.Cell().Element(CellStyle).AlignCenter().Text(item.DispensaryQty.ToString()).FontSize(9);                

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    }
                }
            });

            column.Item().Table(table => {
                // step 1
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(1.2f);            
                    columns.RelativeColumn();                
                    columns.RelativeColumn();                                                
                });

                table.Footer(footer =>
                {
                    footer.Cell().Element(Style).AlignLeft().Text("").FontSize(10);
                    footer.Cell().Element(Style).AlignLeft().Text("").FontSize(10);
                    footer.Cell().Element(Style).AlignCenter().Text($"{Model!.SumTotalQty:N0}").FontSize(10);
                    footer.Cell().Element(Style).AlignCenter().Text($"{Model!.SumTotalDispensary:N0}").FontSize(10);                
                });

                static IContainer Style(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce();
                }
            });       

        });        
    }
}
