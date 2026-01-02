using System.Security.Cryptography;
using Anubis.Database;
using Anubis.Database.Models;
using Anubis.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Anubis.Scanner;

public static class Scales
{
    private static HttpClient _httpClient;

    private static ILogger _logger;
    
    static Scales()
    {
        _httpClient = new HttpClient();
        _logger = LoggingProvider.NewLogger("Anubis.Scales");
    }

    public static async Task<ScalesResponse> AddBlacklist(string url, ulong guildId, uint punishment, int punishmentDuration = 0)
    {
        var ctx = new AnubisContext();
        var responseMessage = await _httpClient.GetAsync(url);
        if (!responseMessage.IsSuccessStatusCode)
        {
            return new ScalesResponse()
            {
               IsSuccess = false,
               Reason = $"Provided URL returned error code: {responseMessage.StatusCode}, {responseMessage.ReasonPhrase}"
            };
        }
        
        var data = await responseMessage.Content.ReadAsByteArrayAsync();
        var hashStr = Convert.ToBase64String(SHA256.HashData(data));

        var wasAdded = false;
        var hash = await ctx.BlacklistedHashes.FirstOrDefaultAsync(x => x.Hash == hashStr);
        if (hash == null)
        {
            hash = new BlacklistedHash()
            {
                Hash = hashStr
            };
            ctx.BlacklistedHashes.Add(hash);
            wasAdded = true;
        }
        
        // Has this hash already been assigned to the guild?
        if (await ctx.HashSettings.FirstOrDefaultAsync(x => x.Guild.Id == guildId && x.Hash.Hash == hashStr) != null)
        {
            return new ScalesResponse()
            {
                IsSuccess = false,
                Reason = "Linked content is already marked as forbidden."
            };
        }

        var hashSetting = new HashSetting()
        {
            Guild = (await GetGuild(ctx, guildId, true))!, // This should never be null with createMissing
            Hash = hash,
            Punishment = punishment,
            PunishmentDuration = punishmentDuration
        };

        ctx.HashSettings.Add(hashSetting);

        await ctx.SaveChangesAsync();
        
        _logger.LogDebug($"Tried adding url '{url}',hash={hashStr},success?={wasAdded}");
        
        return new ScalesResponse()
        {
            IsSuccess = wasAdded,
            Reason = wasAdded? null : "Linked content is already marked as forbidden."
        };
    }

    private static async Task<Guild?> GetGuild(AnubisContext ctx, ulong guildId, bool createMissing = false)
    {
        var guild = await ctx.Guilds.FirstOrDefaultAsync(x => x.Id == guildId);
        if (guild != null || !createMissing) return guild;

        guild = new Guild()
        {
            Id = guildId,
            Hashes = [],
            LogChannel = 0
        };
        ctx.Guilds.Add(guild);
        return guild;
    }
    
    public static async Task<HashSetting?> CheckForbidden(ulong guildId, string url)
    {
        var responseMessage = await _httpClient.GetAsync(url);
        if (!responseMessage.IsSuccessStatusCode) return null;
        
        var data = await responseMessage.Content.ReadAsByteArrayAsync();
        var hashStr = Convert.ToBase64String(SHA256.HashData(data));

        var ctx = new AnubisContext();

        var guild = await ctx.Guilds.Include(guild => guild.Hashes).ThenInclude(hashSetting => hashSetting.Hash).FirstOrDefaultAsync(x => x.Id == guildId);
        var hash = guild?.Hashes.FirstOrDefault(x => x.Hash.Hash == hashStr);
        
        return hash;
    }
}