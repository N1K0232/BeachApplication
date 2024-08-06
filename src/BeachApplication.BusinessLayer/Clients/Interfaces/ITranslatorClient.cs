using BeachApplication.Shared.Models.Responses;
using BeachApplication.Shared.Models.TranslationClient;

namespace BeachApplication.BusinessLayer.Clients.Interfaces;

public interface ITranslatorClient : IDisposable
{
    Task<DetectedLanguageResponse?> DetectLanguageAsync(string input);

    Task<IEnumerable<DetectedLanguageResponse>> DetectLanguagesAsync(IEnumerable<string> input);

    Task<IEnumerable<ServiceLanguage>> GetLanguagesAsync(string? language = null);

    Task<TranslationResponse?> TranslateAsync(string input, IEnumerable<string> to);

    Task<TranslationResponse?> TranslateAsync(string input, string from, IEnumerable<string>? to = null);

    Task<IEnumerable<TranslationResponse>> TranslateAsync(IEnumerable<string> input, string from, IEnumerable<string> to);
}