using BeachApplication.Contracts;
using QRCoder;

namespace BeachApplication.Services;

public class QRCodeGeneratorService(QRCodeGenerator generator, IWebHostEnvironment environment) : IQRCodeGeneratorService
{
    public Task<byte[]> GenerateAsync(string email, string secret)
    {
        var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(environment.ApplicationName)}:{email}?secret={secret}&issuer={Uri.EscapeDataString(environment.ApplicationName)}";

        using var qrCodeData = generator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        var qrCodeBytes = qrCode.GetGraphic(3);
        return Task.FromResult(qrCodeBytes);
    }
}