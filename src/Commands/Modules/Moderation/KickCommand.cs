﻿using System.Net;
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
    public sealed partial class ModerationModule : VolteModule
    {
        [Command("Kick")]
        [Description("Kicks the given user.")]
        [Remarks("Usage: |prefix|kick {@user} [reason]")]
        [RequireBotGuildPermission(GuildPermission.KickMembers)]
        [RequireGuildModerator]
        public async Task<ActionResult> KickAsync(SocketGuildUser user,
            [Remainder] string reason = "Kicked by a Moderator.")
        {
            try
            {
                await Context.CreateEmbed($"You were kicked from **{Context.Guild.Name}** for **{reason}**.")
                    .SendToAsync(user);
            }
            catch (Discord.Net.HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
            {
                Logger.Warn(LogSource.Volte,
                    $"encountered a 403 when trying to message {user}!", e);
            }

            await user.KickAsync(reason);

            return Ok($"Successfully kicked **{user.Username}#{user.Discriminator}** from this server.", _ =>
                ModLogService.DoAsync(ModActionEventArgs.New
                    .WithDefaultsFromContext(Context)
                    .WithActionType(ModActionType.Kick)
                    .WithTarget(user)
                    .WithReason(reason))
                );
        }
    }
}