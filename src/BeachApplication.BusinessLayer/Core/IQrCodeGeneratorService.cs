namespace BeachApplication.BusinessLayer.Core;

public interface IQrCodeGeneratorService
{
    Task<byte[]> GenerateAsync(string qrCodeUri);
}