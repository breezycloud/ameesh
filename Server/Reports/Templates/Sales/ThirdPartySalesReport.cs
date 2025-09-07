using Microsoft.AspNetCore.Components.Routing;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.Reports;

namespace Server.Pages.Reports.Templates.Sales;

public class ThirdPartySalesReport(SalesReportResponse? template) : IDocument
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
                column.Item().Text($"{template!.StoreName}").Style(titleStyle);

                column.Item().Text(text =>
                {                    
                    text.Span("Third Party Sales Report");
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
            column.Item().Element(ComposeFooter);       
        });
    }

    void ComposeFooter(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(15);
                columns.RelativeColumn();
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();               
            });

            table.Footer(footer => {

                footer.Cell().RowSpan(3).ColumnSpan(3).Element(Style).Text("Total").ExtraBlack().ExtraBold().Italic().FontSize(11);
                footer.Cell().Element(Style).AlignRight().Text($"{template!.Summary.TotalAmount:N2}").FontSize(9);
                footer.Cell().Element(Style).AlignRight().Text($"{template!.Summary.TotalDiscount:N2}").FontSize(9);
                footer.Cell().Element(Style).AlignRight().Text($"{template!.Summary.TotalSubTotal:N2}").FontSize(9);
                footer.Cell().Element(Style).AlignRight().Text($"{template!.Summary.TotalAmountPaid:N2}").FontSize(9);
                footer.Cell().Element(Style).AlignRight().Text($"{template!.Summary.TotalAmountDue:N2}").FontSize(9);
                footer.Cell().Element(Style).AlignRight().Text($"{template!.Summary.TotalProfit:N2}").FontSize(9);
                footer.Cell().Element(Style).Text("").FontSize(9);                

                static IContainer Style(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5);
                }
            });
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            // step 1
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(15);
                columns.RelativeColumn();
                columns.RelativeColumn(2.5f);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();                
                columns.RelativeColumn(1.5f);               
            });

            // step 2
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("#").FontSize(9);
                header.Cell().Element(CellStyle).Text("Date").FontSize(9);
                header.Cell().Element(CellStyle).Text("Customer Name").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Total Amount").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Discount").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Sub Total").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Amount Paid").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Amount Due").FontSize(9);                
                header.Cell().Element(CellStyle).AlignRight().Text("Profit").FontSize(9);                
                header.Cell().Element(CellStyle).AlignCenter().Text("Status").FontSize(9);

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });
        

        // step 3
            foreach (var item in template!.ThirdPartySales.DistinctBy(x => x.ReceiptNo))
            {
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span($"{template!.ThirdPartySales.IndexOf(item) + 1}").FontSize(8);
                });
                table.Cell().Element(CellStyle).Text($"{item.Date:dd/MM/yyyy}").FontSize(8);
                table.Cell().Element(CellStyle).Text(item.Customer).FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalAmount:N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Discount:N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.SubTotal:N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.AmountPaid:N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.AmountDue:N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.StoreProfit:N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignCenter().Text(item.Status).FontColor(GetStatusColor(item.Status)).SemiBold().FontSize(8);

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(0);
                }
            }                
        });
        

    }

    private Color GetStatusColor(string status)
    {
        return status switch
        {
            "Pending" => Colors.Red.Darken3,
            "Paid" => Colors.LightGreen.Darken3,
            _ => Colors.Orange.Darken3
        };
    }
}
