using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace CoachFrank.Commands.Attributes
{
    /// <summary>
    /// Allows admin
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RequireAdminRoleAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Guild == null || ctx.Member == null)
                return Task.FromResult(false);

            var result = ctx.Member.Roles.Any(x => x.Name == "Admin");
            return Task.FromResult(result);
        }
    }
}
