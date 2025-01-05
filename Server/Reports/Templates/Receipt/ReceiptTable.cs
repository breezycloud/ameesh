using System.Reflection;
using ApexCharts;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shared.Models.Orders;

namespace Server.Pages.Reports.Templates.Receipt
{
    public class ReceiptTable(string Type, decimal Discount, decimal Paid, decimal Balance, List<OrderItemDetail> items, Order order) : IComponent
    {
        public void Compose(IContainer container)
        {
            container.PaddingVertical(1.2f).Padding(1.2f).Table(table =>
            {
                table.ColumnsDefinition(column =>
                {
                    column.RelativeColumn(0.35f);
                    column.RelativeColumn(2.5f);
                    column.RelativeColumn(1f);
                    // column.RelativeColumn(1f);
                });

                table.Header(header =>
                {
                    header.Cell().AlignCenter().Text("Qty").Bold().FontSize(8);
                    header.Cell().Text("Item").Bold().FontSize(8);
                    header.Cell().AlignRight().Text("Cost").Bold().FontSize(8);                    
                });

                foreach (var item in items)
                {
                    table.Cell().AlignCenter().Text(text =>
                    {
                        text.Span(item.Quantity.ToString()).FontSize(8);
                    });
                    table.Cell().Text(item.ItemName).FontSize(8);
                    table.Cell().AlignRight().Text($"{item.Cost * (item.Quantity is null ? 1 : item.Quantity.GetValueOrDefault()):N2}").FontSize(8);                    
                }
                table.Footer(footer => 
                {                    
                    footer.Cell().RowSpan(3).ColumnSpan(3).Row(row =>
                    {
                        row.RelativeItem(7).AlignRight().Text("Total").FontSize(8);
                        row.RelativeItem(2).AlignRight().Text(GetTotal()).FontSize(8);
                    });
                    if (order.HasDelievery)
                    {
                        footer.Cell().RowSpan(3).ColumnSpan(3).Row(row =>
                        {
                            row.RelativeItem(7).AlignRight().Text($"Delivery Amout:").FontSize(8);                            
                            row.RelativeItem(2).AlignRight().Text(GetDeliveryAmt()).FontSize(8);
                        });                    
                    }
                    footer.Cell().RowSpan(3).ColumnSpan(3).Row(row =>
                    {
                        row.RelativeItem(7).AlignRight().Text("Discount").FontSize(8);
                        row.RelativeItem(2).AlignRight().Text(GetDiscount()).FontSize(8);
                    });
                    footer.Cell().RowSpan(3).ColumnSpan(3).Row(row =>
                    {
                        row.RelativeItem(7).AlignRight().Text("Sub Total").FontSize(8);
                        row.RelativeItem(2).AlignRight().Text(GetGrandTotal()).FontSize(8);
                    });

                    footer.Cell().RowSpan(3).ColumnSpan(3).Row(row =>
                    {
                        row.RelativeItem(7).AlignRight().Text("Amount Paid").FontSize(8);
                        row.RelativeItem(2).AlignRight().Text(GetPayment()).FontSize(8);
                    });
                    footer.Cell().RowSpan(3).ColumnSpan(3).Row(row =>
                    {
                        row.RelativeItem(7).AlignRight().Text("Balance").FontSize(8);
                        row.RelativeItem(2).AlignRight().Text(GetBalance()).FontSize(8);
                    });  
                    footer.Cell().RowSpan(3).ColumnSpan(3).Column(col =>
                    {
                        col.Item().AlignLeft().Text("Note").FontSize(8).Bold();
                        col.Item().AlignLeft().Text(order.Note).FontSize(8);
                    });
                                                          
                });
            });
        }

        private string GetTotal()
        {
            return (items.Sum(x => (x.Quantity is null ? 1 : x.Quantity.GetValueOrDefault()) * x.Cost) + order.DeliveryAmt).ToString("N2");
        }


        private string GetGrandTotal() => (items.Sum(x => (x.Quantity is null ? 1 : x.Quantity.GetValueOrDefault()) * x.Cost) - Discount + + order.DeliveryAmt).ToString("N2");
        private string GetDeliveryAmt() => order.DeliveryAmt.ToString("N2");
        private string GetConsultationNote() => items.Select(x => x.ConsultationNote!).FirstOrDefault(string.Empty);
        private string GetDiscount() => Discount.ToString("N2");
        private string GetPayment() => Paid.ToString("N2");
        private string GetPreviousPayment() => Paid.ToString("N2");
        private string GetBalance() => Balance.ToString("N2");        
    }
}
