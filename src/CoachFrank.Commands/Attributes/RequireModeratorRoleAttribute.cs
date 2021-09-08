using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CoachFrank.Commands.Attributes
{
    /// <summary>
    /// Allows moderator and above to run
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RequireModeratorRoleAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
                return Task.FromResult(false);

            var result = ctx.Member.Roles.Any(x => x.Name == "Moderator" || x.Name == "Admin");
            return Task.FromResult(result);
        }
    }
}
