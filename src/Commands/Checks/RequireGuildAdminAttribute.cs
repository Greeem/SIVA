using System;
using System.Threading.Tasks;
using Gommon;
using Qmmands;

namespace Volte.Commands.Checks
{
    public sealed class RequireGuildAdminAttribute : CheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var ctx = context.Cast<VolteContext>();
            if (ctx.User.IsAdmin(provider)) return CheckResult.Successful;

            await ctx.ReactFailureAsync();
            return CheckResult.Unsuccessful("Insufficient permission.");
        }
    }
}