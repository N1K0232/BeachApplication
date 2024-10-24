using System.Globalization;

namespace System.Reflection;

[AttributeUsage(AttributeTargets.Assembly)]
public class BuildDateTimeAttribute(string value) : Attribute
{
    private const string Format = "yyyyMMddHHmmss";
    private const DateTimeStyles Styles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;

    public DateTime DateTime
    {
        get
        {
            return DateTime.ParseExact(value, Format, CultureInfo.InvariantCulture, Styles);
        }
    }
}