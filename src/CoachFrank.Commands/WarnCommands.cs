using System;
using System.Linq;
using System.Threading.Tasks;
using CoachFrank.Commands.Attributes;
using CoachFrank.Commands.Utils;
using CoachFrank.Data;
using CoachFrank.Data.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace CoachFrank.Commands
{
    [SlashCommandGroup("warn", "Manage Warnings", false)]
    [RequirePermission(Permissions.KickMembers)]
    public class WarnCommands : ApplicationCommandModule
    {
        private readonly BotContext _context;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public WarnCommands(BotContext context)
        {
            _context = context;
        }

        [SlashCommand("add", "Add Warning")]
        public async Task WarnAdd(InteractionContext ctx, [Option("user", "User to warn")]  DiscordUser user, [Option("reason", "Warn reason")]  string reason)
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

            Logger.Info($"Added warning User: {user.Username} Reason: {reason}");

            var count = _context.Warnings.Count(x => x.DiscordId == user.Id && !x.Removed);

            var builder = new DiscordEmbedBuilder
            {
                Title = "Warning saved",
                Color = new Optional<DiscordColor>(DiscordColor.Orange),
                Description = $"{user.Username} has {count} warnings"
            };

            await ctx.CreateResponseAsync(builder);
        }

        [SlashCommand("remove", "Remove Warning")]
        public async Task WarnRemove(InteractionContext ctx, [Option("warning_id", "Warning Id to remove")]  double warnId)
        {
            var warning = _context.Warnings.SingleOrDefault(x => x.Id == (int)warnId);
            if (warning != null)
            {
                warning.Removed = true;
                await _context.SaveChangesAsync();

                Logger.Info($"Removed warning User: {warning.DiscordId} Reason: {warning.Reason}");
                var user = await ctx.Client.GetUserAsync(warning.DiscordId);

                var builder = new DiscordEmbedBuilder
                {
                    Title = $"Warning removed for {user.Username}",
                    Color = new Optional<DiscordColor>(DiscordColor.Orange),
                };
                await ctx.CreateResponseAsync(builder);
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

            var table = new AsciiTable("Id", "Issuer", "Date", "Reason");
            foreach (var warn in warnings)
            {
                var issuer = await ctx.Client.GetUserAsync(warn.IssuerId);
                table.Add(warn.Id.ToString(), issuer.Username, warn.Timestamp.ToShortDateString(), warn.Reason);
            }

            var builder = new DiscordEmbedBuilder
            {
                Title = $"Warnings for {user.Username}",
                Color = new Optional<DiscordColor>(DiscordColor.Orange),
                Description = table.ToString(),
            };
            await ctx.CreateResponseAsync(builder);
        }
    }
}
