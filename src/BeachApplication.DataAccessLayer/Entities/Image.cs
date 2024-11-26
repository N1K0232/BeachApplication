using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Image : TenantEntity
{
    public string Path { get; set; }

    public string ContentType { get; set; }

    public long Length { get; set; }
}