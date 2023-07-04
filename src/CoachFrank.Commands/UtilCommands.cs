using System;
using System.Text;
using System.Threading.Tasks;
using CoachFrank.Commands.Attributes;
using CoachFrank.Commands.RestEase;
using CoachFrank.Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestEase;

namespace CoachFrank.Commands
{
    public class UtilCommands : ApplicationCommandModule
    {
        [SlashCommand("exit", "Kill the bot, should restart")]
        [RequirePermission(Permissions.Administrator)]
        public async Task Exit(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("Restarting!");
            Environment.Exit(1);
        }

        [SlashCommand("ping", "Alive?")]
        public async Task Ping(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(EmojiConstants.PingPong);
        }

        [SlashCommand("uptime", "Get bot uptime")]
        public Task Uptime(InteractionContext ctx)
        {
            var current = System.Diagnostics.Process.GetCurrentProcess();
            var upTime = DateTime.Now - current.StartTime;

            var builder = new DiscordEmbedBuilder
            {
                Title = "Bot Uptime",
                Color = new Optional<DiscordColor>(DiscordColor.Green),
            };
            builder.AddField("Uptime", upTime.Humanize(2));
            builder.AddField("Started", $"{current.StartTime}");

            return ctx.CreateResponseAsync(builder);
        }

        [SlashCommand("status", "Status of the Hall of Meat servers")]
        public async Task Status(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("Under Development");
            return;

            //TODO use DiscordEmbedBuilder
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var statusUpdateResult = new StringBuilder();
            var skateboard3Client = RestClient.For<IServerStatus>("http://sb3.hallofmeat.net");
            try
            {
                var statusUpdate = await skateboard3Client.GetStatus();
                statusUpdateResult.AppendLine($"{EmojiConstants.GreenCircle} Skateboard3Server is Up for {(DateTime.Now - statusUpdate.StartTime).Humanize(2)}");
            }
            catch (Exception)
            {
                statusUpdateResult.AppendLine($"{EmojiConstants.RedCircle} Skateboard3Server is Down");
            }
            var qos1Client = RestClient.For<IServerStatus>("http://sb3-qs1.hallofmeat.net:17502");
            try
            {
                var statusUpdate = await qos1Client.GetStatus();
                statusUpdateResult.AppendLine($"{EmojiConstants.GreenCircle} Skateboard3Server QS1 is Up for {(DateTime.Now - statusUpdate.StartTime).Humanize(2)}");
            }
            catch (Exception)
            {
                statusUpdateResult.AppendLine($"{EmojiConstants.RedCircle} Skateboard3Server QS1 is Down");
            }
            var qos2Client = RestClient.For<IServerStatus>("http://sb3-qs2.hallofmeat.net:17502");
            try
            {
                var statusUpdate = await qos2Client.GetStatus();
                statusUpdateResult.AppendLine($"{EmojiConstants.GreenCircle} Skateboard3Server QS2 is Up for {(DateTime.Now - statusUpdate.StartTime).Humanize(2)}");
            }
            catch (Exception)
            {
                statusUpdateResult.AppendLine($"{EmojiConstants.RedCircle} Skateboard3Server QS2 is Down");
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(statusUpdateResult.ToString()));
        }
    }
}
