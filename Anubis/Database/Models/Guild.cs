using Microsoft.EntityFrameworkCore;

namespace Anubis.Database.Models;

[PrimaryKey(nameof(Id))]
public class Guild
{
    public ulong Id { get; set; }
    public ulong LogChannel { get; set; }
    public required ICollection<HashSetting> Hashes { get; set; }
}