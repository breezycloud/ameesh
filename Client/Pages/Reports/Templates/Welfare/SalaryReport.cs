using Microsoft.AspNetCore.Components.Routing;
using Mud = MudBlazor;
using Shared.Models.Welfare;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.Reports;
using Client.Handlers;

namespace Client.Pages.Reports.Templates.Welfare;

public class SalaryReport(SalaryReportData? Model) : IDocument
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
                    text.Span($"{StringConverter.ConvertToMonth(Model!.Month!.Value)} {Model!.Year!.Value} Salary Report");
                });                
            });            
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

                row.RelativeItem().Element(Style).AlignRight().Text($"{Model!.SumSalaryAmount:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{Model!.SumBonus:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{Model!.SumAdvance:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{Model!.SumPenalty:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{Model!.SumEarnings:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{Model!.SumDeductions:N2}").FontSize(9);
                row.RelativeItem().Element(Style).AlignRight().Text($"{Model!.SumTotal:N2}").FontSize(9);                
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
                columns.RelativeColumn(1.5f);
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
                header.Cell().Element(CellStyle).Text("Customer Name").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Salary Amount").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Bonus").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Advance").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Penalty").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Earnings").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Deductions").FontSize(9);
                header.Cell().Element(CellStyle).AlignRight().Text("Net Pay").FontSize(9);

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).ShowOnce().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            // step 3
            foreach (var item in Model!.Data)
            {
                table.Cell().Element(CellStyle).Text(text =>
                {
                    text.Span($"{Model!.Data.IndexOf(item) + 1}").FontSize(9);
                });                
                table.Cell().Element(CellStyle).Text(item.Staff).FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Amount:N2}").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Bonus:N2}").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Advance:N2}").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Penalty:N2}").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Earnings:N2}").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Deductions:N2}").FontSize(9);
                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Total:N2}").FontSize(9);


                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(0);
                }
            }

        });

    }

}
