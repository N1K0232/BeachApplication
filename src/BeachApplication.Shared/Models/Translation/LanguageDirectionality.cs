using System.Runtime.Serialization;

namespace BeachApplication.Shared.Models.TranslationClient;

public enum LanguageDirectionality
{
    [EnumMember(Value = "ltr")]
    LeftToRight,

    [EnumMember(Value = "rtl")]
    RightToLeft
}