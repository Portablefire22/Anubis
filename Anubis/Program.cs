using Anubis.Config;

namespace Anubis;

class Program
{
    static async Task Main(string[] args)
    {
        var bot = new Bot.Bot(GlobalConfiguration.Discord.Token);
        await bot.Start();
        await Task.Delay(-1);
    }
}