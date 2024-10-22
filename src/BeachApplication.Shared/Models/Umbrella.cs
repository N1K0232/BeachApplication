using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Umbrella : BaseObject
{
    public string Letter { get; set; } = null!;

    public int Number { get; set; }

    public bool IsBusy { get; set; }
}