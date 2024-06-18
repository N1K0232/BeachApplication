namespace BeachApplication.Shared.Models.TranslationClient;

public class DetectedLanguage : DetectedLanguageBase
{
    public bool IsTranslationSupported { get; init; }

    public bool IsTransliterationSupported { get; init; }
}