using QRCoder;

namespace Shared.Helpers;

public class Converter : IConverter
{
    public byte[] ConvertImageToByte(Guid id)
    {
        string baseAddress = $"{id}";
        QRCodeGenerator qr = new();
        QRCodeData codeData = qr.CreateQrCode(baseAddress, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new(codeData);
        byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
        using var ms = new MemoryStream(qrCodeAsBitmapByteArr);
        return ms.ToArray();        
    }
    public byte[] ConvertToByte(string id)
    {
        string baseAddress = $"{id}";
        QRCodeGenerator qr = new();
        QRCodeData codeData = qr.CreateQrCode(baseAddress, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new(codeData);
        byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
        using var ms = new MemoryStream(qrCodeAsBitmapByteArr);
        return ms.ToArray();        
    }
}
