﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gommon
{
    /// <summary>
    ///     Extensions for any class in the System namespace, including sub-namespaces, such as System.Text.
    /// </summary>
    public static partial class Extensions
    {
        public static string ReplaceIgnoreCase(this string str, string toReplace, object replacement)
            => str.Replace(toReplace, replacement.ToString(), StringComparison.OrdinalIgnoreCase);
        
        public static string CalculateUptime(this Process process)
            => (DateTime.Now - process.StartTime).Humanize(3);

        public static Task<IUserMessage> SendFileToAsync(this MemoryStream stream, 
            ITextChannel channel, string text = null, bool isTts = false, Embed embed = null, RequestOptions options = null,
            bool isSpoiler = false, AllowedMentions allowedMentions = null)
        {
            return channel.SendFileAsync(stream, "", text, isTts, embed, options, isSpoiler, allowedMentions);
        }

        public static bool IsMatch(this Regex regex, string str, out Match match)
        {
            match = regex.Match(str);
            return match.Success;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.Any();
    }
}
