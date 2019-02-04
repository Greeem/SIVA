﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Volte.Core.Commands.Modules.Utility {
    public partial class UtilityModule : VolteModule {
        [Command("Avatar")]
        [Summary("Shows the mentioned user's avatar, or yours if no one is mentioned.")]
        [Remarks("Usage: $avatar [@user]")]
        public async Task Avatar(SocketGuildUser user = null) {
            var embed = CreateEmbed(Context, string.Empty).ToEmbedBuilder();
            if (user is null) {
                embed.WithImageUrl(Context.User.GetAvatarUrl());
            }
            else {
                embed.WithImageUrl(user.GetAvatarUrl());
            }
            
            await Reply(Context.Channel, embed.Build());

        }
    }
}