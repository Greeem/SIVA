﻿using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SIVA.Helpers;

namespace SIVA.Core.Discord.Modules.General {
    public class SuggestCommand : SivaCommand {
        [Command("Suggest")]
        public async Task Suggest() {
            await Context.Channel.SendMessageAsync("", false,
                Utils.CreateEmbed(
                    Context,
                    "You can suggest bot features [here](https://goo.gl/forms/i6pgYTSnDdMMNLZU2)."
                )
            );
        }
    }
}