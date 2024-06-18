using BeachApplication.Shared.Models.TranslationClient;

namespace BeachApplication.Shared.Models.Responses;

public record class DetectedLanguageResponse(IEnumerable<DetectedLanguage> Alternatives);