using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoachFrank.Commands.Attributes;
using CoachFrank.Data;
using CoachFrank.Data.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoachFrank.Commands
{
    [Group("warn")]
    public class WarnCommands : BaseCommandModule
    {
        private readonly BotContext _context;

        public WarnCommands(BotContext context)
        {
            _context = context;
        }

        [GroupCommand]
        public async Task Warn(CommandContext ctx, DiscordUser user, [RemainingText] string reason = "")
        {
            var warning = new Warning
            {
                DiscordId = user.Id,
                IssuerId = ctx.User.Id,
                Reason = reason,
                Timestamp = DateTime.UtcNow
            };
            _context.Warnings.Add(warning);
            await _context.SaveChangesAsync();

            var count = _context.Warnings.Count(x => x.DiscordId == user.Id && !x.Removed);

            await ctx.RespondAsync($"Warning saved, user has {count} warnings!");
        }

        [Command("remove")]
        [RequireAdminRole]
        public async Task WarnRemove(CommandContext ctx, int warnId)
        {
            var warning = _context.Warnings.SingleOrDefault(x => x.Id == warnId);
            if (warning != null)
            {
                warning.Removed = true;
                await _context.SaveChangesAsync();
                await ctx.RespondAsync($"Warning {warnId} removed");
            }
        }

        [Command("list")]
        [RequireAdminRole, RequireModeratorRole]
        public async Task WarnList(CommandContext ctx, DiscordUser user)
        {
            var warnings = await _context.Warnings.Where(x => x.DiscordId == user.Id && !x.Removed).ToListAsync();
            if(!warnings.Any())
            {
                await ctx.RespondAsync("User has no warnings");
                return;
            }
            var builder = new StringBuilder();
            builder.Append($"Id\t\tIssuer\t\tTimestamp\t\tReason\n");
            foreach (var warn in warnings)
            {
                var issuer = await ctx.Client.GetUserAsync(warn.IssuerId);
                builder.Append($"{warn.Id}\t{issuer.Username}\t{warn.Timestamp.ToShortDateString()}\t{warn.Reason}");
            }
            await ctx.RespondAsync(builder.ToString());
        }
    }
}
