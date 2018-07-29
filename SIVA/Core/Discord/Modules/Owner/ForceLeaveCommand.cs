﻿using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SIVA.Helpers;

namespace SIVA.Core.Discord.Modules.Owner {
    public class ForceLeaveCommand : SivaCommand {
        [Command("ForceLeave")]
        public async Task ForceLeave([Remainder] string serverName) {
            if (!UserUtils.IsBotOwner(Context.User)) {
                await Context.Message.AddReactionAsync(new Emoji(RawEmoji.X));
                return;
            }

            var target = Siva.GetInstance().Guilds.FirstOrDefault(g => g.Name == serverName);
            if (target == null) {
                await Context.Channel.SendMessageAsync("", false,
                    Utils.CreateEmbed(Context, $"I'm not in the guild **{serverName}**."));
                return;
            }

            await target.LeaveAsync();
            await Context.Channel.SendMessageAsync("", false,
                Utils.CreateEmbed(Context, $"Successfully left {target.Name}"));
        }
    }
}