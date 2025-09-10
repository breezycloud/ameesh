using Microsoft.AspNetCore.Components.Routing;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.Reports;
using Shared.Models.Orders;

namespace Server.Pages.Reports.Templates.Sales;

public class SalesWithThirdParty(ThirdpartySalesReportTemplate template) : IDocument
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
                column.Item().Text($"{template!.StoreName}").Style(titleStyle);

                column.Item().Text(text =>
                {                    
                    text.Span("Thirdparty Sales Report");
                });

                column.Item().Text(text =>
                {
                    text.Span($"{(template!.Criteria == "Date" ? "Date: " : "From: ")}").SemiBold();
                    text.Span(template!.StartDate);
                });                
                if (template!.Criteria == "Range")
                {
                    column.Item().Text(text =>
                    {
                        text.Span("To: ").SemiBold();
                        text.Span(template!.EndDate);
                    });
                }
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
                row.RelativeItem().Element(Style).Text("").FontSize(9);
                row.RelativeItem().Element(Style).Text("").FontSize(9);                
                row.RelativeItem().Element(Style).Text("").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumBuyPrice:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumSellPrice:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumTotalAmount:N2}").FontSize(9);                                
            });
            column.Item().AlignRight().Row(row => 
            {
                row.AutoItem().AlignRight().Text($"Expected Profit {template!.SumProfit:N2}").FontSize(11).Bold();
            });

            static IContainer Style(IContainer container)
            {
                return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5);
            }
        });
    }
    
    void ComposeTable (IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(5);
            foreach (var item in template!.SalesReport)
            {
                column.Item().Row(row => 
                {
                    row.AutoItem().Column(col =>
                    {
                        col.Item().Text($"Receipt #: {item.ReceiptNo}").FontSize(9).Bold();
                        col.Item().Text($"Customer: {item.Customer}").FontSize(9).Bold();
                    });                    
                });
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("#").FontSize(9);
                        header.Cell().Element(CellStyle).Text("Item").FontSize(9);
                        header.Cell().Element(CellStyle).AlignCenter().Text("Qty").FontSize(9);
                        header.Cell().Element(CellStyle).AlignRight().Text("Cost").FontSize(9);
                        header.Cell().Element(CellStyle).AlignRight().Text("Price").FontSize(9);
                        header.Cell().Element(CellStyle).AlignRight().Text("Total").FontSize(9);
                        
                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        }
                    });

                    foreach (var saleItem in item.SaleItems.OrderByDescending(x => x.Total))
                    {
                        table.Cell().Element(CellStyle).Text(text =>
                        {
                            text.Span($"{item.SaleItems.IndexOf(saleItem) + 1}").FontSize(8);
                        });
                        table.Cell().Element(CellStyle).Text(saleItem.ItemName).FontSize(8);
                        table.Cell().Element(CellStyle).AlignCenter().Text(saleItem.Quantity.ToString()).FontSize(8);
                        table.Cell().Element(CellStyle).AlignRight().Text($"{saleItem.Cost:N2}").FontSize(8);
                        table.Cell().Element(CellStyle).AlignRight().Text($"{saleItem.Price:N2}").FontSize(8);
                        table.Cell().Element(CellStyle).AlignRight().Text($"{saleItem.Total:N2}").FontSize(8);

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(0);
                        }
                    }                    
                });                                
            }
            
        });        
    }
}
