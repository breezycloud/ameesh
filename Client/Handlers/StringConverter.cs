using System.Globalization;
using System.Text;

namespace Client.Handlers;

class StringConverter
{
    public static string ConvertToTitleCase(string value)
    {
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
    }

    public static decimal ConvertToDecimal(string value)
    {
        return decimal.Parse(value);
    }

    public static string ConvertToMonth(int value)
    {
        return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(value);
    }

    public static string ConvertToBase64(Stream stream)
    {
        byte[] bytes;
        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            bytes = memoryStream.ToArray();
        }

        return Convert.ToBase64String(bytes);        
    }
}