using System.Text.RegularExpressions;
using Anubis.Config;
using Anubis.Database.Models;
using Anubis.Logging;
using Anubis.Scanner;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace Anubis.Bot;

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
      _client.MessageCreate += OnMessageCreate;
   }

   private async ValueTask OnMessageCreate(Message message)
   {
      var content = message.Content;
      if (message.GuildId == null) return;
      var wasMatch = false;
      var matches = GlobalConfiguration.UrlRegex.Matches(content);
      HashSetting? hash = null;
      foreach (Match match in matches)
      {
         var str = match.Value;
         hash = await Scales.CheckForbidden((ulong)message.GuildId, str);
         if (hash != null)
         {
            wasMatch = true;
            break;
         }
      }

      if (!wasMatch || hash == null) return;
      await message.DeleteAsync();

      var bitfield = hash.Punishment;
      
      // Punishments
      if ((bitfield & (uint)HashPunishment.DirectMessage) == 1)
      {
         await SendWarningInDm(message.Author);
      }
      
      if ((bitfield & (uint)HashPunishment.Ban) != 0)
      {
         await _client.Rest.BanGuildUserAsync(hash.Guild.Id, message.Author.Id);
      } 
      else if ((bitfield & (uint)HashPunishment.Timeout) != 0)
      {
         var user = await _client.Rest.GetGuildUserAsync(hash.Guild.Id, message.Author.Id);
         var duration = hash.PunishmentDuration == -1 ? TimeSpan.FromDays(365 * 30) : TimeSpan.FromMinutes(hash.PunishmentDuration);
         await user.TimeOutAsync(DateTimeOffset.UtcNow + duration);
      }
      
      if (hash.Guild.LogChannel != 0)
      {
         await SendModLog(hash.Guild.LogChannel, message);
      }
   }

   private async Task SendModLog(ulong logChannel, RestMessage original)
   {
      await _client.Rest.SendMessageAsync(logChannel, new MessageProperties()
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

      var us = await _client.Rest.GetCurrentUserAsync();
      _logger.LogInformation($"Bot initiated as '{us.Username}#{us.Discriminator}'");
   }
   
}