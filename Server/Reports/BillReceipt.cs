using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Client.Pages.Reports;

public class BillReceipt(byte[] Image, string? Type, string? ReceiptNo)
{
    public byte[] Create()
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(9f, 9f, Unit.Centimetre);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

            });
        });
        return document.GeneratePdf();
    }

    public IEnumerable<byte[]> GetImage()
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(9f, 9f, Unit.Centimetre);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

            });
        });
        return document.GenerateImages();
    }
    
    void ComposeHeader(IContainer container)
    {        
        container.AlignCenter().Column(column =>
        {
            column.Item().AlignCenter().Text("Ameesh Luxury").ExtraBold().FontSize(15);
            column.Item().AlignCenter().Text($"Invoice #: {ReceiptNo}").Bold().FontSize(10);
        });

    }

    void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            column.Item()
                  .AlignCenter()
                  .Width(30, Unit.Millimetre)
                  .Height(30, Unit.Millimetre)
                  .AlignCenter()
                  .Image(Image)
                  .FitArea();

            column.Item()
                  .AlignCenter()
                  .Text("Please present this to the Cashier")
                  .FontSize(10);
            
            if (Type == "Store")
            {                
                column.Item()
                  .AlignCenter()
                  .Text("VALID FOR 1 HOUR")
                  .Bold()
                  .FontSize(15);
            }            
        });
    }
}
