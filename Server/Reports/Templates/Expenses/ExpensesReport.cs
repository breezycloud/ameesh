using Microsoft.AspNetCore.Components.Routing;

using Shared.Models.Welfare;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.Reports;
using Client.Handlers;
using Shared.Models.Expenses;
using Shared.Enums;
using MudBlazor.Extensions;

namespace Server.Pages.Reports.Templates.Welfare;

public class ExpenseReport(ExpenseData[] Model) : IDocument
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
                column.Item().Text($"Ameesh Luxury").Style(titleStyle);

                column.Item().Text(text =>
                {                    
                    text.Span($"Expense Report");
                });                
            });            
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(5).Column(column =>
        {
            column.Spacing(2);

            //table
            column.Item().Element(ComposeTable);            
            //footer
            column.Item().Element(ComposeFooter);  

                
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
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();                
            });

            // step 2
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("#").FontSize(9);
                header.Cell().Element(CellStyle).AlignLeft().Text("Date").FontSize(9);
                header.Cell().Element(CellStyle).AlignLeft().Text("Expense").FontSize(9);
                header.Cell().Element(CellStyle).AlignLeft().Text("Description").FontSize(9);
                header.Cell().Element(CellStyle).AlignLeft().Text("Reference").FontSize(9);
                header.Cell().Element(CellStyle).AlignLeft().Text("User").FontSize(9);
                header.Cell().Element(CellStyle).AlignCenter().Text("Payment Method").FontSize(9);                
                header.Cell().Element(CellStyle).AlignRight().Text("Amount").FontSize(9);

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            // step 3
            int index = 0;
            foreach (var item in Model!)
            {
                index++;
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span($"{index}").FontSize(9);
                });                
                table.Cell().Element(CellStyle).Text(item.date.ToString("dd MM yyyy")).FontSize(9);
                table.Cell().Element(CellStyle).Text(item.expense).FontSize(9);
                table.Cell().Element(CellStyle).Text(item.reference).FontSize(9);
                table.Cell().Element(CellStyle).Text(item.description).FontSize(9);
                table.Cell().Element(CellStyle).Text(item.user).FontSize(9);                
                table.Cell().Element(CellStyle).AlignCenter().Text(item.mode.ToString()).FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text(item.amount.ToString("N2")).FontSize(9);                


                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(0);
                }
            }

        });

    }

    void ComposeFooter (IContainer container)
    {
        container.Table(table =>
        {
            // step 1
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();                
            });

            // step 2
            table.Footer(footer =>
            {                
                footer.Cell().RowSpan(8).ColumnSpan(8).AlignRight().Text(Model.Sum(x => x.amount).ToString("N2")).FontSize(9);
            });            

        });

    }

}
