using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Comment : TenantEntity
{
    public Guid UserId { get; set; }

    public int Score { get; set; }

    public string Title { get; set; }

    public string Text { get; set; }
}