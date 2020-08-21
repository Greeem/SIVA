﻿using System.Threading.Tasks;
using Qmmands;
using Volte.Commands.Results;

namespace Volte.Commands.Modules
{
    public sealed partial class AdminModule
    {
        [Command("ServerPrefix", "Sp", "GuildPrefix", "Gp")]
        [Description("Sets the command prefix for this guild.")]
        [Remarks("serverprefix {String}")]
        public Task<ActionResult> ServerPrefixAsync([Remainder]string newPrefix)
        {
            ModifyData(data =>
            {
                data.Configuration.CommandPrefix = newPrefix;
                return data;
            });
            return Ok($"Set this guild's prefix to **{newPrefix}**.");
        }
    }
}