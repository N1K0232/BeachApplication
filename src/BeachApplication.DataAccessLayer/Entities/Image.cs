using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Image : BaseEntity
{
    public string Path { get; set; }

    public long Length { get; set; }

    public string ContentType { get; set; }

    public string Description { get; set; }
}