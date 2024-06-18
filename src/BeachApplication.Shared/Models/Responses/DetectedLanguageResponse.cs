using BeachApplication.Shared.Models.Translation;

namespace BeachApplication.Shared.Models.Responses;

public record class DetectedLanguageResponse(IEnumerable<DetectedLanguage> Alternatives);