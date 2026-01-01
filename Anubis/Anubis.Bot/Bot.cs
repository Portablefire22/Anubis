using System.Text.RegularExpressions;
using Anubis.Anubis.Config;
using Anubis.Anubis.Logging;
using Anubis.Anubis.Scanner;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Logging;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace Anubis.Anubis.Bot;

public class Bot
{
   private GatewayClient _client;
   private ApplicationCommandService<ApplicationCommandContext> _commandService;

   private ILogger _logger;
   
   public Bot(string token)
   {
      _logger = LoggingProvider.NewLogger<Bot>();
      _client = new GatewayClient(
         new BotToken(token),
         new GatewayClientConfiguration()
         {
            // Logger = new ConsoleLogger()
            Intents = GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent
         });
      _commandService = new ApplicationCommandService<ApplicationCommandContext>();
   }

   private async Task RegisterCommands()
   {
      _commandService.AddSlashCommand(new SlashCommandBuilder("hello", "Hello, World!", () => "Hello, World!"));
      _commandService.AddModules(typeof(Program).Assembly); 
      _client.InteractionCreate += async interaction =>
      {
         if (interaction is not ApplicationCommandInteraction applicationCommandInteraction)
            return;

         var result = await _commandService.ExecuteAsync(new ApplicationCommandContext(applicationCommandInteraction, _client));

         if (result is not IFailResult failResult)
            return;

         try
         {
            await interaction.SendResponseAsync(InteractionCallback.Message(failResult.Message));
         }
         catch
         {
         }
      };
      await _commandService.RegisterCommandsAsync(_client.Rest, _client.Id);
   }

   public void RegisterEvents()
   {
      _client.MessageCreate += async message =>
      {
         var content = message.Content;
         var wasMatch = false;
         var matches = GlobalConfiguration.UrlRegex.Matches(content);
         foreach (Match match in matches)
         {
            var str = match.Value;
            if (await Scales.CheckForbidden(str))
            {
               wasMatch = true;
               break;
            }
         }

         if (!wasMatch) return;
         await message.DeleteAsync();

         await SendModLog(message);
         await SendWarningInDm(message.Author);
      };
   }

   private async Task SendModLog(RestMessage original)
   {
      await _client.Rest.SendMessageAsync(original.ChannelId, new MessageProperties()
      {
         Content = GlobalConfiguration.Discord.GetModLogMessage(original.Author)
      });
   }
   
   private async Task SendWarningInDm(User recipient)
   {
      var directChannel = await recipient.GetDMChannelAsync();
      var directMessage = GlobalConfiguration.Discord.GetDirectMessage(recipient);
      await directChannel.SendMessageAsync(directMessage);
   }
   
   public async Task Start()
   {
      await RegisterCommands();
      RegisterEvents();

      await _client.StartAsync();
      _logger.LogInformation("Bot Started.");
   }
   
}