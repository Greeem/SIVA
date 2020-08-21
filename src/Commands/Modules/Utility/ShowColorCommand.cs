﻿using System.Threading.Tasks;
using Qmmands;
using SixLabors.ImageSharp.PixelFormats;
using Volte.Commands.Results;
using System.Text;
using DSharpPlus.Entities;
using Gommon;

namespace Volte.Commands.Modules
{
    public sealed partial class UtilityModule
    {
        [Command("ShowColor", "Sc")]
        [Description("Shows an image purely made up of the specified color.")]
        [Remarks("showcolor {Color}")]
        public Task<ActionResult> ShowColorAsync([Remainder] DiscordColor color)
            => Ok(async () =>
            {
                await using var stream = new Rgba32(color.R, color.G, color.B).CreateColorImage();
                await stream.SendFileToAsync(Context.Channel, "role.png", false, new DiscordEmbedBuilder()
                    .WithColor(color)
                    .WithTitle($"Color {color}")
                    .WithDescription(new StringBuilder()
                        .AppendLine($"**Hex:** {color.ToString().ToUpper()}")
                        .AppendLine($"**RGB:** {color.R}, {color.G}, {color.B}")
                        .ToString())
                    .WithImageUrl("attachment://role.png")
                    .WithCurrentTimestamp()
                    .Build());
            });
    }
}
