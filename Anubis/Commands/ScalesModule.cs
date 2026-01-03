using System.Globalization;
using Anubis.Database;
using Anubis.Database.Models;
using Anubis.Scanner;
using NetCord;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace Anubis.Commands;

public class ScalesModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    [SlashCommand("blacklistcontent", "Blacklists a URL's content.", 
        DefaultGuildPermissions = Permissions.ManageMessages,
        Contexts = [InteractionContextType.Guild])]
    public async Task<string> BlacklistContent(
        [SlashCommandParameter(Description = "URL of content to blacklist")] string url,
        [SlashCommandParameter(Description = "Punishment Bitfield")] uint punishment,
        [SlashCommandParameter(Description = "Punishment Duration, -1 for perma")] int duration = 0)
    {
        var guildId = Context.Interaction.GuildId!;
        var resp = await Scales.AddBlacklist(url, (ulong)guildId, punishment, duration);
        return resp.IsSuccess ? "Success!" : resp.Reason;
    }

    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    [SlashCommand("setlogchannel", "Sets the specified channel to the bot's log output.",
        DefaultGuildPermissions = Permissions.ManageChannels,
        Contexts = [InteractionContextType.Guild])]
    public async Task<string> SetLogChannel(
        [SlashCommandParameter(Description = "Channel ID for log output")] string channelId = "")
    {
        var nullOrEmpty = string.IsNullOrEmpty(channelId);
        ulong finalId;
        if (!nullOrEmpty)
        {
            if (!ulong.TryParse(channelId, out finalId)) return $"Invalid channel id `{channelId}`, must be an unsigned long.";
        }
        else
        {
            finalId = Context.Channel.Id;
        }
        
        var guildId = Context.Interaction.GuildId!;
        var ctx = new AnubisContext();
        var guildModel = (await Scales.GetGuild(ctx, (ulong)guildId, true))!;
        ctx.Update(guildModel);
        guildModel.LogChannel = finalId;
        await ctx.SaveChangesAsync();
        
        return $"Log channel has been set to <#{finalId}>";
    }
}