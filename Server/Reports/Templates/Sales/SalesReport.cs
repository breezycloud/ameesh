using Microsoft.AspNetCore.Components.Routing;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.Reports;

namespace Server.Pages.Reports.Templates.Sales;

public class SalesReport(SalesReportTemplate? template) : IDocument
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
                    text.Span("Sales Report");
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
                row.RelativeItem().Element(Style).Text("").FontSize(9);

                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumTotalAmount:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumDiscount:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumSubTotal:N2}").FontSize(9);
                // row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumAmountPaid:N2}").FontSize(9);
                // row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumRefund:N2}").FontSize(9);
                // row.RelativeItem().Element(Style).AlignRight().Text($"{(template!.SumAmountPaid - template!.SumRefund):N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumProfit:N2}").FontSize(9);                
                row.RelativeItem().Element(Style).AlignRight().Text($"{template!.SumTP:N2}").FontSize(9);
                // row.RelativeItem().Element(Style).Text("").FontSize(9);                
            });
            column.Item().AlignRight().Row(row =>
            {                
                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Element(Style).Text($"Note: TP (Third party)").FontSize(7);
                    col.Item().AlignRight().Element(Style).Text($"Grand Total").Italic().Bold();
                });
                row.RelativeItem().Element(Style).Text("").FontSize(9);
                row.RelativeItem().Element(Style).Text("").FontSize(9);
                row.RelativeItem().Element(Style).Text("").FontSize(9);
                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Element(Style).Text($"{template!.SumTP:N2}").FontSize(9);
                    col.Item().AlignRight().Element(Style).Text($"{template!.SumTotalAmount + template!.SumTP:N2}").FontSize(9);
                });
                row.RelativeItem().Element(Style).Text("").FontSize(9);
                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Element(Style).Text($"{template!.SumTP:N2}").FontSize(9);
                    col.Item().AlignRight().Element(Style).Text($"{template!.SumSubTotal + template!.SumTP:N2}").FontSize(9);
                });
                row.RelativeItem().Element(Style).Text("").FontSize(9);
                row.RelativeItem().Element(Style).Text("").FontSize(9);
            });

            static IContainer Style(IContainer container)
            {
                return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5);
            }
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
                columns.RelativeColumn(0.8f);
                columns.RelativeColumn(0.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                // columns.RelativeColumn();
                // columns.RelativeColumn();
                // columns.RelativeColumn();
                // columns.RelativeColumn();
            });

            // step 2
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("#").FontSize(9);
                header.Cell().Element(CellStyle).Text("Receipt No").FontSize(9);
                header.Cell().Element(CellStyle).Text("Date").FontSize(9);
                header.Cell().Element(CellStyle).Text("Customer Name").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Total Amount").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Discount").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Sub Total").FontSize(9);
                // header.Cell().Element(CellStyle).AlignRight().Text("Paid").FontSize(9);
                // header.Cell().Element(CellStyle).AlignRight().Text("Refund").FontSize(9);
                // header.Cell().Element(CellStyle).AlignRight().Text("Net Amount").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Profit").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("TP").FontSize(9);
                // header.Cell().Element(CellStyle).AlignCenter().Text("Remarks").FontSize(9);

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            // step 3
            foreach (var item in template!.SaleItems.DistinctBy(x=> x.ReceiptNo).OrderByDescending(x => x.Date))
            {
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span($"{template!.SaleItems.IndexOf(item) + 1}").FontSize(8);
                });
                table.Cell().Element(CellStyle).Text(item.ReceiptNo).FontSize(8);
                table.Cell().Element(CellStyle).Text($"{item.Date:dd/MM/yyyy}").FontSize(8);
                table.Cell().Element(CellStyle).Text(item.Customer).FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalAmount:N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Discount:N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.SubTotal:N2}").FontSize(8);
                // table.Cell().Element(CellStyle).AlignRight().Text($"{item.AmountPaid:N2}").FontSize(8);
                // table.Cell().Element(CellStyle).AlignRight().Text($"{item.Refund:N2}").FontSize(8);
                // table.Cell().Element(CellStyle).AlignRight().Text($"{(item.AmountPaid - item.Refund):N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Profit:N2}").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.TP_Total:N2}").FontSize(8);                
                // table.Cell().Element(CellStyle).AlignCenter().Text(item.GetRemark()).FontSize(8);
                //if (item.GetRemark().Contains("F & F"))


                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(0);
                }
            }

        });

    }

}
