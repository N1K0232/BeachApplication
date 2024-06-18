using BeachApplication.Shared.Models.TranslationClient;

namespace BeachApplication.Shared.Models.Responses;

public class TranslationResponse
{
    public DetectedLanguageBase DetectedLanguage { get; init; } = null!;

    public IEnumerable<Translation> Translations { get; init; } = [];

    public Translation? Translation => Translations?.FirstOrDefault();
}