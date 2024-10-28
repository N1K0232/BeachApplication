namespace BeachApplication.Contracts;

public interface IQRCodeGeneratorService
{
    Task<byte[]> GenerateAsync(string email, string secret);
}