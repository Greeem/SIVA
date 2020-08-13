using System.Net.Http;
using System.Threading;
using Volte.Core.Attributes;
using Volte.Services;
// ReSharper disable MemberCanBePrivate.Global

namespace Volte.Commands.Modules
{
    [RequireBotOwner]
    public sealed partial class BotOwnerModule : VolteModule
    {
        public EvalService Eval { get; set; }
        public HttpClient Http { get; set; }

    }
}