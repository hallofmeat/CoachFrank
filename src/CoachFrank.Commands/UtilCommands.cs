using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;

namespace CoachFrank.Commands
{
    public class UtilCommands : BaseCommandModule
    {
        [Command("ping")]
        public Task Ping(CommandContext ctx)
        {
            return ctx.RespondAsync(":ping_pong:");
        }

        [Command("status")]
        public Task Status(CommandContext ctx)
        {
            var current = System.Diagnostics.Process.GetCurrentProcess();
            var upTime = DateTime.Now - current.StartTime;
            return ctx.RespondAsync($"Uptime: {upTime.Humanize(2)}");
        }
    }
}
