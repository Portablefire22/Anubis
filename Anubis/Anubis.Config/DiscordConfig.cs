using NetCord;

namespace Anubis.Abubis.Config;

public class DiscordConfig
{
    public string Token { get; set; }

    public Dictionary<string, string> Messages { get; set; }

    public string GetDirectMessage(User recipient)
    {
        var raw = Messages["DirectMessage"];
        var str = string.Format(raw, recipient.GlobalName);
        return str;
    }
    
    public string GetModLogMessage(User recipient)
    {
        var raw = Messages["ModLog"];
        var str = string.Format(raw, recipient.Id);
        return str;
    }
}