using Discord;
using Discord.WebSocket;
using Volte.Core;
using Volte.Data.Models.Guild;
using Volte.Services;

namespace Volte.Data.Models.EventArgs
{
    public sealed class UserLeftEventArgs : System.EventArgs
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        public IGuildUser User { get; }
        public IGuild Guild { get; }
        public GuildData Data { get; }
        public DiscordSocketClient Client { get; }

        public UserLeftEventArgs(IGuildUser user)
        {
            User = user;
            Guild = user.Guild;
            Data = _db.GetData(Guild);
            Client = VolteBot.Client;
        }
    }
}