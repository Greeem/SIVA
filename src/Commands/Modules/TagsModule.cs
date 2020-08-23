using System.Linq;
using System.Threading.Tasks;
using Gommon;
using Humanizer;
using Qmmands;
using Volte.Commands.Checks;
using Volte.Commands.Results;
using Volte.Core.Models.Guild;

namespace Volte.Commands.Modules
{
    [Group("Tags", "T")]
    public sealed class TagsModule : VolteModule {
        [Command("Get")]
        [Description("Gets a tag's contents if it exists.")]
        [Remarks("tags get {Tag}")]
        public Task<ActionResult> TagAsync([Remainder]Tag tag)
        {
            tag.Uses += 1;
            Db.UpdateData(Context.GuildData);

            if (Context.GuildData.Configuration.EmbedTagsAndShowAuthor)
            {
                return Ok(Context.CreateEmbedBuilder(tag.FormatContent(Context)).WithFooter($"Requested by {Context.Member}."), async message =>
                {
                    if (Context.GuildData.Configuration.DeleteMessageOnTagCommandInvocation)
                    {
                        await Context.Message.TryDeleteAsync();
                    }
                });
            }

            return Ok(tag.FormatContent(Context), async message =>
            {
                if (Context.GuildData.Configuration.DeleteMessageOnTagCommandInvocation)
                {
                    await Context.Message.TryDeleteAsync();
                }
            }, false);

        }

        [Command("Stats")]
        [Description("Shows stats for a tag.")]
        [Remarks("tags stats {Tag}")]
        public async Task<ActionResult> TagStatsAsync([Remainder]Tag tag)
        {
            var u = await Context.Client.GetShardFor(Context.Guild).GetUserAsync(tag.CreatorId);

            return Ok(Context.CreateEmbedBuilder()
                .WithTitle($"Tag {tag.Name}")
                .AddField("Response", $"`{tag.Response}`", true)
                .AddField("Creator", $"{u}", true)
                .AddField("Uses", $"**{tag.Uses}**", true));
        }

        [Command("List")]
        [Description("Lists all available tags in the current guild.")]
        [Remarks("tags list")]
        public Task<ActionResult> TagsAsync()
        {
            var tagsList = Context.GuildData.Extras.Tags;
            if (tagsList.IsEmpty()) return BadRequest("This guild doesn't have any tags.");
            else
            {
                return None(async () =>
                {
                    await Context.Interactivity.SendPaginatedMessageAsync(Context.Channel, Context.Member,
                        tagsList.Select(x => $"`{x.Name}`").GetPages(10));
                }, false);
            }
        }

        [Command("Create", "Add", "New")]
        [Description("Creates a tag with the specified name and response (in that order).")]
        [Remarks("tags create {String} {String}")]
        [RequireGuildModerator]
        public async Task<ActionResult> TagCreateAsync(string name, [Remainder] string response)
        {
            var tag = Context.GuildData.Extras.Tags.FirstOrDefault(t => t.Name.EqualsIgnoreCase(name));
            if (tag is not null)
            {
                var user = await Context.Client.GetShardFor(Context.Guild).GetUserAsync(tag.CreatorId);
                return BadRequest(
                    $"Cannot make the tag **{tag.Name}**, as it already exists and is owned by **{user}**.");
            }

            tag = new Tag
            {
                Name = name,
                Response = response,
                CreatorId = Context.Member.Id,
                GuildId = Context.Guild.Id,
                Uses = default
            };

            ModifyData(data =>
            {
                data.Extras.Tags.Add(tag);
                return data;
            });

            return Ok(Context.CreateEmbedBuilder()
                .WithTitle("Tag Created!")
                .AddField("Name", tag.Name)
                .AddField("Response", tag.Response)
                .AddField("Creator", Context.Member.Mention));
        }

        [Command("Delete", "Del", "Rem")]
        [Description("Deletes a tag if it exists.")]
        [Remarks("tags delete {Tag}")]
        [RequireGuildModerator]
        public async Task<ActionResult> TagDeleteAsync([Remainder]Tag tag)
        {
            ModifyData(data =>
            {
                data.Extras.Tags.Remove(tag);
                return data;
            });
            return Ok($"Deleted the tag **{tag.Name}**, created by " +
                      $"**{await Context.Client.ShardClients.First().Value.GetUserAsync(tag.CreatorId)}**, with " +
                      $"**{"use".ToQuantity(tag.Uses)}**.");
        }

        public Task<ActionResult> TagEditAsync(Tag tag, [Remainder] string content)
        {
            tag.Response = content;
            ModifyData(data =>
            {
                data.Extras.Tags.Remove(tag);
                data.Extras.Tags.Add(tag);
                return data;
            });
            
            return Ok(Context.CreateEmbedBuilder()
                .WithTitle("Tag Updated")
                .AddField("Name", tag.Name)
                .AddField("Response", content)
                .AddField("Creator", Context.Member.Mention)
                .AddField("Uses", tag.Uses));
        }
    }
}