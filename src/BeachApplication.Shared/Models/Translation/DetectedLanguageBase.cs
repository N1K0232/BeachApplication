namespace BeachApplication.Shared.Models.TranslationClient;

public abstract class DetectedLanguageBase
{
    public string Language { get; init; } = null!;

    public float Score { get; init; }

    public override string ToString() => Language;
}