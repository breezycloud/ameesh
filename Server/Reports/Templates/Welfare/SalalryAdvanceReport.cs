using Client.Handlers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.Welfare;

namespace Server.Pages.Reports.Templates.Welfare;

public class SalaryAdvanceReport(SalaryAdvanceReportData Model) : IDocument
{
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
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
                column.Item().Text("Ameesh Luxury").Style(titleStyle);
                column.Item().Text($"{StringConverter.ConvertToMonth(Model.Month!.Value)} {Model.Year!.Value} Salary Advance Report");
            });
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

    void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn(1f);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("#").FontSize(9);
                header.Cell().Element(HeaderCell).Text("Date").FontSize(9);
                header.Cell().Element(HeaderCell).Text("Staff").FontSize(9);
                header.Cell().Element(HeaderCell).AlignRight().Text("Amount").FontSize(9);
                header.Cell().Element(HeaderCell).Text("Comment").FontSize(9);

                static IContainer HeaderCell(IContainer c) =>
                    c.DefaultTextStyle(x => x.SemiBold())
                     .PaddingVertical(5)
                     .BorderBottom(1)
                     .BorderColor(Colors.Black);
            });

            foreach (var item in Model.Data)
            {
                var index = Model.Data.IndexOf(item) + 1;
                table.Cell().Element(Cell).Text(index.ToString()).FontSize(9);
                table.Cell().Element(Cell).Text(item.Date).FontSize(9);
                table.Cell().Element(Cell).Text(item.Staff).FontSize(9);
                table.Cell().Element(Cell).AlignRight().Text($"{item.Amount:N2}").FontSize(9);
                table.Cell().Element(Cell).Text(item.Comment ?? "").FontSize(9);

                static IContainer Cell(IContainer c) =>
                    c.BorderBottom(1)
                     .BorderColor(Colors.Grey.Lighten2)
                     .PaddingVertical(2);
            }
        });
    }

    void ComposeFooter(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Text("").FontSize(9);
                header.Cell().Text("").FontSize(9);
                header.Cell().Text("").FontSize(9);
                header.Cell().AlignRight().Text("").FontSize(9);
                header.Cell().Text("").FontSize(9);                
            });

            // Rows
            table.Cell().Element(Cell).Text("").FontSize(9);
            table.Cell().Element(Cell).Text("").FontSize(9);
            table.Cell().Element(Cell).Text("").FontSize(9);
            table.Cell().Element(Cell).AlignRight().Text($"{Model.Data.Sum(d => d.Amount):N2}").FontSize(9);
            table.Cell().Element(Cell).Text("").FontSize(9);

            static IContainer Cell(IContainer c) =>
                c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2);            
        });
    }
}
