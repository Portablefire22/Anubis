using Anubis.Scanner;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace Anubis.Commands;

public class ScalesModule : ApplicationCommandModule<ApplicationCommandContext>
{
    
    [SlashCommand("blacklistcontent", "Blacklists a URL's content.", 
        DefaultGuildPermissions = Permissions.ManageMessages,
        Contexts = [InteractionContextType.Guild])]
    public static async Task<string> BlacklistContent(
        [SlashCommandParameter(Description = "URL of content to blacklist")] string url)
    {
        var resp = await Scales.AddImage(url);

        return resp.IsSuccess ? "Success!" : resp.Reason;
    }
}