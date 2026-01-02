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
        [SlashCommandParameter(Description = "Punishment Duration, -1 for perma")] int? duration)
    {
        var guildId = Context.Interaction.GuildId!;
        duration ??= 0;
        var resp = await Scales.AddBlacklist(url, (ulong)guildId, punishment, (int)duration);
        return resp.IsSuccess ? "Success!" : resp.Reason;
    }
}