using BeachApplication.Authentication.Entities;
using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Comment : BaseEntity
{
    public Guid UserId { get; set; }

    public int Score { get; set; }

    public string Title { get; set; }

    public string Text { get; set; }

    public virtual ApplicationUser User { get; set; }
}