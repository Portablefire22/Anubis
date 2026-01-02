using Microsoft.EntityFrameworkCore;

namespace Anubis.Database.Models;

[PrimaryKey(nameof(Id))]
public class HashSetting
{
    public int Id { get; set; }
    public required Guild Guild { get; set; }
    public required BlacklistedHash Hash { get; set; }
    public uint Punishment { get; set; }
    public int PunishmentDuration { get; set; } = 0;
}

public enum HashPunishment : uint
{
    None = 0,
    DirectMessage = 1,  // Tell the user in DMs that they probably have malware
    Timeout = 1 << 1,
    Ban = 1 << 2,
}