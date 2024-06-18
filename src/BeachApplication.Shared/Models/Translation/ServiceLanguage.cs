namespace BeachApplication.Shared.Models.TranslationClient;

public class ServiceLanguage
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string NativeName { get; set; } = null!;

    public LanguageDirectionality Directionality { get; set; }

    public override string ToString() => Name;
}