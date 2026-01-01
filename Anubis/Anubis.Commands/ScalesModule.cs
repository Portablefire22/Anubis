using Anubis.Anubis.Scanner;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Anubis.Anubis.Commands;

public class ScalesModule : ApplicationCommandModule<ApplicationCommandContext>
{
    
    [SlashCommand("addimage", "Add image :3")]
    public static async Task<string> AddImage(
        [SlashCommandParameter(Description = "URL of content to blacklist")] string url)
    {
        var resp = await Scales.AddImage(url);

        return resp.IsSuccess ? "Success!" : resp.Reason;
    }
}