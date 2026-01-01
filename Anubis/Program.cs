using Anubis.Abubis.Config;
using Anubis.Anubis.Bot;
using Anubis.Anubis.Config;

namespace Anubis;

class Program
{
    static async Task Main(string[] args)
    {
        var bot = new Bot(GlobalConfiguration.Discord.Token);
        await bot.Start();
        await Task.Delay(-1);
    }
}