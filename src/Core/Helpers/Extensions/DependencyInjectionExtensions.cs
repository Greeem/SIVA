﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Qmmands;
using RestSharp;
using Volte;
using Volte.Services;
using Version = Volte.Version;

namespace Gommon
{
    public static partial class Extensions
    {
        public static IServiceCollection AddAllServices(this IServiceCollection coll, int shardCount) =>
            //add all other services; formerly in the VolteBot class
            coll.AddVolteServices()
                .AddSingleton(new RestClient {UserAgent = $"Volte/{Version.FullVersion}"})
                .AddSingleton<HttpClient>()
                .AddSingleton<CancellationTokenSource>()
                .AddSingleton(new CommandService(new CommandServiceConfiguration
                {
                    IgnoresExtraArguments = true,
                    Separator = " "
                }))
                .AddSingleton(new DiscordShardedClient(new DiscordSocketConfig
                {
                    LogLevel = Version.ReleaseType is Version.DevelopmentStage.Development
                        ? LogSeverity.Debug
                        : LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    ConnectionTimeout = 10000,
                    MessageCacheSize = 50,
                    GatewayIntents = GatewayIntents.Guilds | 
                                     GatewayIntents.GuildMembers | 
                                     GatewayIntents.GuildMessages | 
                                     GatewayIntents.GuildMessageReactions |
                                     GatewayIntents.GuildPresences,
                    TotalShards = shardCount
                }));

        public static IServiceCollection AddVolteServices(this IServiceCollection coll)
        {
            //get all the classes that are Volte[Event]Services, aren't abstract, and don't have the System.ObsoleteAttribute attribute.
            foreach (var service in typeof(Program).Assembly.GetTypes()
                .Where(t => !t.HasAttribute<ObsoleteAttribute>() && (
                            t.Inherits<VolteEventService>() 
                            || t.Inherits<VolteService>()) && !t.IsAbstract))
            {
                coll.TryAddSingleton(service);
            }

            return coll;
        }

        public static T Get<T>(this IServiceProvider provider)
            => provider.GetRequiredService<T>();

        public static void Get<T>(this IServiceProvider provider, out T service)
            => service = provider.GetRequiredService<T>();
    }
}