using QRCoder;

namespace BeachApplication.BusinessLayer.Core;

public class QrCodeGeneratorService(QRCodeGenerator generator) : IQrCodeGeneratorService
{
    public Task<byte[]> GenerateAsync(string qrCodeUri)
    {
        using var qrCodeData = generator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        var qrCodeBytes = qrCode.GetGraphic(3);
        return Task.FromResult(qrCodeBytes);
    }
}