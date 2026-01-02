using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Anubis.Config;

public class GlobalConfiguration
{
    public static DiscordConfig Discord { get; private set; }
    
    public static IConfigurationRoot Config { get; private set; }
    
    public static Regex UrlRegex { get; private set; }

    static GlobalConfiguration()
    {
        Config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        Discord = Config.GetRequiredSection("Discord").Get<DiscordConfig>()!;

        UrlRegex = new Regex(
            // ".*https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)"
            @"(http|ftp|https):\/\/([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:\/~+#-]*[\w@?^=%&\/~+#-])"
            );
    } 
}