using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Gommon;
using Humanizer;
using Qmmands;
using Volte.Commands.Checks;
using Volte.Commands.Results;
using Volte.Core.Models.Guild;

namespace Volte.Commands.Modules
{
    public sealed partial class ModerationModule : VolteModule
    {
        [Command("TagCreate", "TagAdd", "TagNew")]
        [Priority(1)]
        [Description("Creates a tag with the specified name and response.")]
        [Remarks("Usage: |prefix|tagcreate {name} {response}")]
        [RequireGuildModerator]
        public Task<ActionResult> TagCreateAsync(string name, [Remainder] string response)
        {
            var tag = Context.GuildData.Extras.Tags.FirstOrDefault(t => t.Name.EqualsIgnoreCase(name));
            if (tag != null)
            {
                var user = Context.Client.GetUser(tag.CreatorId);
                return BadRequest(
                    $"Cannot make the tag **{tag.Name}**, as it already exists and is owned by {user.Mention}.");
            }

            var newTag = new Tag
            {
                Name = name,
                Response = response,
                CreatorId = Context.User.Id,
                GuildId = Context.Guild.Id,
                Uses = 0
            };

            Context.GuildData.Extras.Tags.Add(newTag);
            Db.UpdateData(Context.GuildData);

            return Ok(Context.CreateEmbedBuilder()
                .WithTitle("Tag Created!")
                .AddField("Name", newTag.Name)
                .AddField("Response", newTag.Response)
                .AddField("Creator", Context.User.Mention));
        }

        [Command("TagDelete", "TagDel", "TagRem")]
        [Priority(1)]
        [Description("Deletes a tag if it exists.")]
        [Remarks("Usage: |prefix|tagdelete {name}")]
        [RequireGuildModerator]
        public Task<ActionResult> TagDeleteAsync([Remainder] string name)
        {
            var tag = Context.GuildData.Extras.Tags.FirstOrDefault(t => t.Name.EqualsIgnoreCase(name));
            if (tag is null)
                return BadRequest($"Cannot delete the tag **{name}**, as it doesn't exist.");

            var user = Context.Client.GetUser(tag.CreatorId);

            Context.GuildData.Extras.Tags.Remove(tag);
            Db.UpdateData(Context.GuildData);
            return Ok($"Deleted the tag **{tag.Name}**, created by " +
                      $"{(user != null ? user.Mention : $"user with ID **{tag.CreatorId}**")} with **{tag.Uses}** " +
                      $"{"use".ToQuantity(tag.Uses)}.");
        }
    }
}