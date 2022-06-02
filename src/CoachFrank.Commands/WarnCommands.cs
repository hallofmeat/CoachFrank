using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoachFrank.Data;
using CoachFrank.Data.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.EntityFrameworkCore;

namespace CoachFrank.Commands
{
    [SlashCommandGroup("warn", "Manage Warnings", false)]
    [SlashRequirePermissions(Permissions.KickMembers)]
    public class WarnCommands : ApplicationCommandModule
    {
        private readonly BotContext _context;

        public WarnCommands(BotContext context)
        {
            _context = context;
        }

        [SlashCommand("add", "Add Warning")]
        public async Task WarnAdd(InteractionContext ctx, [Option("user", "User to warn")]  DiscordUser user, [Option("reason", "Warn reason")]  string reason = "")
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

            await ctx.CreateResponseAsync($"Warning saved, user has {count} warnings!");
        }

        [SlashCommand("remove", "Remove Warning")]
        public async Task WarnRemove(InteractionContext ctx, [Option("warningId", "Warning Id to remove")]  double warnId)
        {
            var warning = _context.Warnings.SingleOrDefault(x => x.Id == warnId);
            if (warning != null)
            {
                warning.Removed = true;
                await _context.SaveChangesAsync();
                await ctx.CreateResponseAsync($"Warning {warnId} removed");
            }
        }

        [SlashCommand("list", "List Warnings")]
        public async Task WarnList(InteractionContext ctx, [Option("user", "User to list warnings for")] DiscordUser user)
        {
            var warnings = await _context.Warnings.Where(x => x.DiscordId == user.Id && !x.Removed).ToListAsync();
            if(!warnings.Any())
            {
                await ctx.CreateResponseAsync("User has no warnings");
                return;
            }
            var builder = new StringBuilder();
            builder.Append($"Id\t\tIssuer\t\tTimestamp\t\tReason\n");
            foreach (var warn in warnings)
            {
                var issuer = await ctx.Client.GetUserAsync(warn.IssuerId);
                builder.Append($"{warn.Id}\t{issuer.Username}\t{warn.Timestamp.ToShortDateString()}\t{warn.Reason}\n");
            }
            await ctx.CreateResponseAsync(builder.ToString());
        }
    }
}
