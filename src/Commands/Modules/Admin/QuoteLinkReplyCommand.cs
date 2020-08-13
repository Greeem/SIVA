using System.Threading.Tasks;
using Qmmands;
using Volte.Commands.Results;

namespace Volte.Commands.Modules
{
    public partial class AdminModule
    {
        [Command("QuoteLinkReply", "QuoteLink", "QuoteReply", "JumpUrlReply", "Qrl", "Qlr")]
        [Description("Enables or disables the Quote link parsing and sending into a channel that a 'Quote URL' is posted to for this guild.")]
        [Remarks("quotelinkreply {Boolean}")]
        public Task<ActionResult> QuoteLinkReplyCommandAsync(bool enabled)
        {
            ModifyData(data =>
            {
                data.Extras.AutoParseQuoteUrls = enabled;
                return data;
            });
            return Ok(enabled ? "Enabled QuoteLinkReply for this guild." : "Disabled QuoteLinkReply for this guild.");
        }   
    }
}