using Microsoft.EntityFrameworkCore;

namespace Anubis.Database.Models;

[PrimaryKey(nameof(Id))]
public class BlacklistedHash
{
    public int Id { get; set; }
    public required string Hash { get; set; }

}