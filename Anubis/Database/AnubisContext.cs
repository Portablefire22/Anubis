using Anubis.Config;
using Anubis.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Anubis.Database;

public class AnubisContext : DbContext
{
    public DbSet<BlacklistedHash> BlacklistedHashes { get; set; } = default!;
    public DbSet<Guild> Guilds { get; set; } = default!;
    public DbSet<HashSetting> HashSettings { get; set; } = default!;
   public string DbPath { get; }

   public AnubisContext()
   {
       DbPath = GlobalConfiguration.Config["DbPath"]!;
   }
   
   protected override void OnConfiguring(DbContextOptionsBuilder options)
      => options.UseSqlite($"Data Source={DbPath}"); 
}