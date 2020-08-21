using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;
using Volte.Commands.Checks;
using Volte.Core.Models;
using Volte.Core.Models.EventArgs;
using Volte.Commands.Results;
using Gommon;

namespace Volte.Commands.Modules
{
    public sealed partial class ModerationModule
    {
        [Command("Softban")]
        [Description("Softbans the mentioned user, kicking them and deleting the last x (where x is defined by the daysToDelete parameter) days of messages.")]
        [Remarks("softban {Member} [Int] [String]")]
        [RequireBotGuildPermission(GuildPermission.KickMembers | GuildPermission.BanMembers)]
        public async Task<ActionResult> SoftBanAsync([CheckHierarchy] SocketGuildUser user, int daysToDelete = 0,
            [Remainder] string reason = "Softbanned by a Moderator.")
        {
            if (!await user.TrySendMessageAsync(
                embed: Context.CreateEmbed($"You've been softbanned from **{Context.Guild.Name}** for **{reason}**.")))
            {
                Logger.Warn(LogSource.Volte,
                    $"encountered a 403 when trying to message {user}!");
            }

            await user.BanAsync(daysToDelete == 0 ? 7 : daysToDelete, reason);
            await Context.Guild.RemoveBanAsync(user);

            return Ok($"Successfully softbanned **{user.Username}#{user.Discriminator}**.", _ =>
                ModLogService.DoAsync(ModActionEventArgs.New
                    .WithDefaultsFromContext(Context)
                    .WithTarget(user)
                    .WithReason(reason))
                );
        }
    }
}