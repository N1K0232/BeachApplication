using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Image : BaseObject
{
    public string Path { get; set; } = null!;

    public long Length { get; set; }

    public string ContentType { get; set; } = null!;

    public string? Description { get; set; } = null!;
}