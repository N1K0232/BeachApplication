using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Comment : BaseObject
{
    public int Score { get; set; }

    public string Title { get; set; } = null!;

    public string Text { get; set; } = null!;

    public User User { get; set; } = null!;
}