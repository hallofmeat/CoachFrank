using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoachFrank.Commands.RestEase;
using CoachFrank.Commands.Utils;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using RestEase;

namespace CoachFrank.Commands
{
    public class UtilCommands : BaseCommandModule
    {
        [Command("ping")]
        public Task Ping(CommandContext ctx)
        {
            return ctx.RespondAsync(":ping_pong:");
        }

        [Command("uptime")]
        public Task Uptime(CommandContext ctx)
        {
            var current = System.Diagnostics.Process.GetCurrentProcess();
            var upTime = DateTime.Now - current.StartTime;
            return ctx.RespondAsync($"Bot Started: {current.StartTime}\nBot Uptime: {upTime.Humanize(2)}");
        }

        [Command("status")]
        public async Task Status(CommandContext ctx)
        {
            //TODO do async
            var statusUpdateResult = new StringBuilder();
            var skateboard3Client = RestClient.For<IServerStatus>("http://skateboard3.hallofmeat.net");
            try
            {
                var statusUpdate = await skateboard3Client.GetStatus();
                statusUpdateResult.AppendLine($"{EmojiConstants.CheckMark} Skateboard3Server is Up for {(DateTime.Now - statusUpdate.StartTime).Humanize(2)}");
            }
            catch (Exception)
            {
                statusUpdateResult.AppendLine($"{EmojiConstants.X} Skateboard3Server is Down");
            }
            var qos1Client = RestClient.For<IServerStatus>("http://sb3-qs1.hallofmeat.net:17502");
            try
            {
                var statusUpdate = await qos1Client.GetStatus();
                statusUpdateResult.AppendLine($"{EmojiConstants.CheckMark} Skateboard3Server QS1 is Up for {(DateTime.Now - statusUpdate.StartTime).Humanize(2)}");
            }
            catch (Exception)
            {
                statusUpdateResult.AppendLine($"{EmojiConstants.X} Skateboard3Server QS1 is Down");
            }
            var qos2Client = RestClient.For<IServerStatus>("http://sb3-qs2.hallofmeat.net:17502");
            try
            {
                var statusUpdate = await qos2Client.GetStatus();
                statusUpdateResult.AppendLine($"{EmojiConstants.CheckMark} Skateboard3Server QS2 is Up for {(DateTime.Now - statusUpdate.StartTime).Humanize(2)}");
            }
            catch (Exception)
            {
                statusUpdateResult.AppendLine($"{EmojiConstants.X} Skateboard3Server QS2 is Down");
            }

            await ctx.RespondAsync(statusUpdateResult.ToString());
        }
    }
}
