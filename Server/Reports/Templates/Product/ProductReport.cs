using Microsoft.AspNetCore.Components.Routing;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.Reports;
using Shared.Models.Products;

namespace Server.Pages.Reports.Templates.Product;

public class ProductReport(ProductReportTemplate? template) : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;
    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4.Landscape());
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
                column.Item().Text(template?.StoreName).Style(titleStyle);

                column.Item().Text(text =>
                {
                    text.Span("Product Report");
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
            column.Item().AlignRight().Row(row =>
            {
                row.RelativeItem().Element(Style).Text("").FontSize(14);
                row.RelativeItem().Element(Style).Text("").FontSize(14);
                row.RelativeItem().Element(Style).Text("").FontSize(14);
                row.RelativeItem().Element(Style).Text($"{template!.SumDispensaryQty:N0}").FontSize(10);
                row.RelativeItem().Element(Style).Text($"{template!.SumStoreQty:N0}").FontSize(10);
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumCostPrice:N2}").FontSize(10);                
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumSellPrice:N2}").FontSize(10);
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumTotalCostPrice:N2}").FontSize(10);
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumProjection:N2}").FontSize(10);
            });

            static IContainer Style(IContainer container)
            {
                return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5);
            }
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            // step 1
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            // step 2
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("#");
                header.Cell().Element(CellStyle).Text("Brand");                
                header.Cell().Element(CellStyle).Text("Product Name");
                header.Cell().Element(CellStyle).Text("Dispensary");
                header.Cell().Element(CellStyle).Text("Store");               
                header.Cell().Element(CellStyle).AlignRight().Text("Cost Price");                
                header.Cell().Element(CellStyle).AlignRight().Text("Sell Price");
                header.Cell().Element(CellStyle).AlignRight().Text("Total Cost Price");
                header.Cell().Element(CellStyle).AlignRight().Text("Total Sell Price");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            // step 3
            var data = template!.Items.AsParallel().Where(x => x.Projection > 0).ToList();
            foreach (var item in data)
            {
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span($"{template!.Items.IndexOf(item) + 1}").FontSize(9);
                });
                table.Cell().Element(CellStyle).Text(item.BrandName).FontSize(9);
                table.Cell().Element(CellStyle).Text(item.ProductName).FontSize(9);
                table.Cell().Element(CellStyle).Text(item.DispensaryQuantity.ToString()).FontSize(9);
                table.Cell().Element(CellStyle).Text(item.StoreQuantity.ToString()).FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.CostPrice:N2}").FontSize(9);                
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.SellPrice:N2}").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalCostPrice:N2}").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Projection:N2}").FontSize(9);

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
        });
    }
}
