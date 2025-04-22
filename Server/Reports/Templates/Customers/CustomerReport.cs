using Microsoft.AspNetCore.Components.Routing;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.Customers;
using Shared.Models.Reports;

namespace Server.Pages.Reports.Templates.Customers;

public class CustomerReport(List<Customer>? template, CustomerFilterType type) : IDocument
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
                column.Item().Text($"Customers").Style(titleStyle);                

                column.Item().Text(text =>
                {
                    // text.Span($"{(! == "Date" ? "Date: " : "From: ")}").SemiBold();                    
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

    void ComposeTable (IContainer container)
    {
        container.Table(table =>
        {
            // step 1
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(0.5f);
                columns.RelativeColumn(0.5f);
                columns.RelativeColumn();
                columns.RelativeColumn();                
            });

            // step 2
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("#").FontSize(9);                
                header.Cell().Element(CellStyle).Text("Customer Name").FontSize(9);
                header.Cell().Element(CellStyle).AlignCenter().Text("Phone No 1").FontSize(9);
                header.Cell().Element(CellStyle).AlignCenter().Text("Phone No 2").FontSize(9);
                header.Cell().Element(CellStyle).AlignLeft().Text("Contact Address 1").FontSize(9);
                header.Cell().Element(CellStyle).AlignLeft().Text("Contact Address 2").FontSize(9);

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            // step 3
            foreach (var item in template)
            {
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span($"{template!.IndexOf(item) + 1}").FontSize(8);
                });
                table.Cell().Element(CellStyle).Text(item.CustomerName).FontSize(8);
                table.Cell().Element(CellStyle).AlignCenter().Text(item.PhoneNo).FontSize(8);
                table.Cell().Element(CellStyle).AlignCenter().Text(item.PhoneNo2).FontSize(8);
                table.Cell().Element(CellStyle).AlignLeft().Text(item.ContactAddress).FontSize(8);
                table.Cell().Element(CellStyle).AlignLeft().Text(item.ContactAddress2).FontSize(8);

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(0);
                }
            }

        });

    }

}
