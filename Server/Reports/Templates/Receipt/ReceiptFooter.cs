using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shared.Helpers;
using Shared.Models.Reports;

namespace Server.Pages.Reports.Templates.Receipt
{
    public class FooterContent(Guid footer) : IComponent
    {
        private int pictureSize = 25;
        public void Compose(IContainer container)
        {
            container.AlignCenter().Column(column =>
            {
                column.Item()
                      .AlignCenter()
                      .Width(pictureSize, Unit.Millimetre)
                      .Height(pictureSize, Unit.Millimetre)
                      .AlignCenter()
                      .Image(new Converter().ConvertImageToByte(footer)!, ImageScaling.FitArea);
            });
        }
    }
}
