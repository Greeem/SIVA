using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Gommon;
using Qmmands;
using Volte.Commands;
using Volte.Core.Helpers;

namespace Volte.Core.Entities
{
    public sealed class RequireBotGuildPermissionAttribute : CheckAttribute
    {
        private readonly GuildPermission[] _permissions;
        
        public IEnumerable<GuildPermission> Permissions => _permissions.ToList();

        public RequireBotGuildPermissionAttribute(params GuildPermission[] perms) => _permissions = perms;

        public override async ValueTask<CheckResult> CheckAsync(CommandContext context)
        {
            var ctx = context.Cast<VolteContext>();
            foreach (var perm in ctx.Guild.CurrentUser.GuildPermissions.ToList())
            {
                if (ctx.Guild.CurrentUser.GuildPermissions.Administrator)
                    return CheckResult.Successful;
                if (_permissions.Contains(perm))
                    return CheckResult.Successful;
            }

            await ctx.CreateEmbedBuilder()
                .AddField("Error in Command", ctx.Command.Name)
                .AddField("Error Reason", $"I am missing the following guild-level permissions required to execute this command: `{ _permissions.Select(x => x.ToString()).Join(", ")}`")
                .SendToAsync(ctx.Channel);
            return CheckResult.Failed("Insufficient permission.");
        }
    }
}