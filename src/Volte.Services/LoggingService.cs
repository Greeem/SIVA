﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colorful;
using DSharpPlus;
using DSharpPlus.Entities;
using Gommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volte.Core;
using Volte.Core.Entities;
using Color = System.Drawing.Color;
using Console = Colorful.Console;

namespace Volte.Services
{
    public sealed class LoggingService : ILogger<BaseDiscordClient>, ILoggerFactory
    {
        private readonly object _lock;
        private const string LogFile = "data/Volte.log";

        public LoggingService()
        {
            _lock = new object();
        }

        internal void PrintVersion()
        {
            Info(LogSource.Volte, "--------------------------------------------");
            new Figlet().ToAscii("Volte").ConcreteValue.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(asciiLine =>
            {
                Info(LogSource.Volte, asciiLine);
            });
            Info(LogSource.Volte, "--------------------------------------------");
            Info(LogSource.Volte, $"Currently running Volte V{Version.FullVersion}.");
        }

        private void Log(LogLevel s, LogSource from, string message, Exception e = null)
        {
            lock (_lock)
            {
                if (s is LogLevel.Debug)
                {
                    if (!Config.EnableDebugLogging) return;
                }

                Execute(s, from, message, e);
            }
        }

        /// <summary>
        ///     Prints a <see cref="LogLevel.Trace"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        public void Debug(LogSource src, string message) 
            => Log(LogLevel.Debug, src, message);

        /// <summary>
        ///     Prints a <see cref="LogLevel.Information"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        public void Info(LogSource src, string message)
            => Log(LogLevel.Information, src, message);

        /// <summary>
        ///     Prints a <see cref="LogLevel.Error"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        /// <param name="e">Optional Exception to print.</param>
        public void Error(LogSource src, string message, Exception e = null)
            => Log(LogLevel.Error, src, message, e);
        /// <summary>
        ///     Prints a <see cref="LogLevel.Critical"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        /// <param name="e">Optional Exception to print.</param>
        public void Critical(LogSource src, string message, Exception e = null)
            => Log(LogLevel.Critical, src, message, e);

        /// <summary>
        ///     Prints a <see cref="LogLevel.Warning"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message, with the specified <paramref name="e"/> exception if provided.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        /// <param name="e">Optional Exception to print.</param>
        public void Warn(LogSource src, string message, Exception e = null)
            => Log(LogLevel.Warning, src, message, e);
        /// <summary>
        ///     Prints a <see cref="LogLevel.Information"/> message to the console from the specified <paramref name="src"/> source, with the given <paramref name="message"/> message.
        /// </summary>
        /// <param name="src">Source to print the message from.</param>
        /// <param name="message">Message to print.</param>
        public void Verbose(LogSource src, string message)
            => Log(LogLevel.Information, src, message);

        /// <summary>
        ///     Prints a <see cref="LogLevel.Error"/> message to the console from the specified <paramref name="e"/> exception.
        /// </summary>
        /// <param name="e">Exception to print.</param>
        public void Exception(Exception e, IServiceProvider provider = null)
            => Execute(LogLevel.Critical, LogSource.Volte, string.Empty, e, provider);
        
        
        private void Execute(LogLevel s, LogSource src, string message, Exception e, IServiceProvider provider = null)
        {
            var content = new StringBuilder();

            var (color, value) = VerifySeverity(s);
            Append($"{value}:".PadRight(10), color);
            var dto = DateTimeOffset.UtcNow;
            content.Append($"[{dto.FormatDate()} | {dto.FormatFullTime()}] {value} -> ");

            (color, value) = VerifySource(src);
            Append($"[{value}]".PadRight(15), color);
            content.Append($"{value} -> ");

            if (!message.IsNullOrWhitespace())
            {
                Append(message, Color.White);
                content.Append(message);
            }

            if (e is not null)
            {
                var toWrite = new StringBuilder($"{e.GetType()}: {e.Message}{Environment.NewLine}{e.StackTrace}");

                var cause = e;
                while ((cause = cause.InnerException) != null)
                {
                    toWrite.Append($"{Environment.NewLine}Caused by {cause.GetType()}: {cause.Message}{Environment.NewLine}{cause.StackTrace}");
                }

                Append(toWrite.ToString(), Color.IndianRed);
                content.Append(toWrite);
                
                Console.WriteLine(); // End the line before LogExceptionInDiscord as it can log to console.
                content.AppendLine();

                if (provider is not null)
                {
                    LogExceptionInDiscord(e, provider);
                }
                
            }
            else
            {
                Console.WriteLine();
                content.AppendLine();
            }

            if (Config.EnabledFeatures.LogToFile)
            {
                File.AppendAllText(LogFile, content.ToString());
            }
        }

        private void Append(string m, Color c)
        {
            Console.ForegroundColor = c;
            Console.Write(m);
        }

        private (Color Color, string Source) VerifySource(LogSource source)
        {
            var sourceAttr = LogSourceAttribute.LogSources[source];
            return (sourceAttr.Color, sourceAttr.Name);
        }


        private (Color Color, string Level) VerifySeverity(LogLevel severity) =>
            severity switch
            {
                LogLevel.Critical => (Color.Maroon, "CRITICAL"),
                LogLevel.Error => (Color.DarkRed, "ERROR"),
                LogLevel.Warning => (Color.Yellow, "WARN"),
                LogLevel.Information => (Color.SpringGreen, "INFO"),
                LogLevel.Trace => (Color.SandyBrown, "TRACE"),
                LogLevel.Debug => (Color.SandyBrown, "TRACE"),
                LogLevel.None => (Color.Chocolate, "NONE"),
                _ => throw new InvalidOperationException($"The specified {nameof(LogLevel)} ({severity}) is invalid.")
            };

        private void LogExceptionInDiscord(Exception e, IServiceProvider provider)
        {
            var client = provider.Get<DiscordShardedClient>();
            var http = provider.Get<HttpService>();

            if (!Config.GuildLogging.EnsureValidConfiguration(client, out var channel))
            {
                Error(LogSource.Volte, "Could not send an exception report to Discord as the GuildLogging configuration is invalid.");
                return;
            }

            _ = Task.Run(async () =>
            {
                var url = await http.PostToGreemPasteAsync(e.StackTrace);
                await new DiscordEmbedBuilder()
                    .WithErrorColor()
                    .WithTitle(
                        $"Exception at {DateTimeOffset.UtcNow.FormatDate()}, {DateTimeOffset.UtcNow.FormatFullTime()} UTC")
                    .AddField("Exception Type", e.GetType().AsPrettyString(), true)
                    .AddField("Exception Message", e.Message, true)
                    .WithDescription($"View the full Stack Trace [here]({url}).")
                    .SendToAsync(channel);
            });
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                Log(logLevel, eventId.GetSource(), state.ToString(), exception);
            }

        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel is LogLevel.Trace && !(Version.ReleaseType is Version.DevelopmentStage.Development ||
                Config.EnableDebugLogging)) return false;

            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new ServiceCollection().BuildServiceProvider();
        }

        public void Dispose()
        {
            
        }

        public ILogger CreateLogger(string _) => this;

        public void AddProvider(ILoggerProvider provider)
        {

        }
    }
}