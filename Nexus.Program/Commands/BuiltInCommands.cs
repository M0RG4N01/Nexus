﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Nexus.Utilities;

namespace Nexus.Commands
{
    public class BuiltInCommands : BaseCommandModule
    {
        [Command("Icons")]
        public async Task Icons(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = "Icons Copyright",
                    Url = "https://icons8.com/",
                },
                Color = DiscordColor.PhthaloGreen,
                Description = "**All icons goto their respective owners from https://icons8.com**",
                Url = "https://icons8.com/",
                Timestamp = DateTimeOffset.Now,
                Title = "Icons Copyright"
            };

            await ctx.RespondAsync(embed: embed);
        }
        
        [Command("author")]
        [Aliases("alexr03", "creator")]
        public async Task Author(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = "Alexr03",
                    Url = "https://alexr03.dev",
                    IconUrl =
                        "https://cdn.discordapp.com/avatars/183270722548793344/c70eb5e8cb08e7f158227386aac9b972.png?size=128"
                },
                Color = DiscordColor.Red,
                Description = "**The author of this super amazing Nexus Bot!**",
                Url = "https://alexr03.dev",
                Timestamp = DateTimeOffset.Now,
                ThumbnailUrl =
                    "https://cdn.discordapp.com/avatars/183270722548793344/c70eb5e8cb08e7f158227386aac9b972.png?size=128",
                Title = "Nexus Author"
            };
            embed.AddField("Github", "https://github.com/Alexr03", true);
            embed.AddField("Gitlab", "https://gitlab.openshift.alexr03.dev/Alex", true);
            embed.AddField("Website", "https://Alexr03.dev", true);
            embed.AddField("Steam", "https://steamcommunity.com/id/alexred03", true);
            embed.AddField("Discord", "Alexr03#1525", true);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("LYHME")]
        public async Task Lyhme(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "LYHME",
                    Url = "https://lyhmehosting.com",
                    IconUrl =
                        "https://cdn.discordapp.com/attachments/190120046134034432/547496887930978334/nodrips.png"
                },
                Color = DiscordColor.Green,
                Description = "Early Adopter of the **Nexus Bot**!",
                Url = "https://lyhmehosting.com",
                Timestamp = DateTimeOffset.Now,
                ThumbnailUrl =
                    "https://cdn.discordapp.com/attachments/190120046134034432/547496887930978334/nodrips.png",
                Title = "LYHME Inc."
            };
            embed.AddField("LYHME.IO", "https://LYHME.io", true);
            embed.AddField("LYHME Hosting", "https://LYHMEHosting.com", true);
            embed.AddField("LYHME Panel", "https://LYHMEPanel.com", true);
            embed.AddField("LYHME Community", "https://lyhme.net/", true);
            
            await ctx.RespondAsync(embed: embed);
        }

        [Command("Modules")]
        public async Task ShowModules(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var plugin in NexusInformation.PluginRepository.AssemblyModules.Plugins)
            {
                assemblies.Add(plugin.GetType().Assembly);
            }

            List<Page> pages = new List<Page>();
            foreach (var assembly in assemblies)
            {
                pages.Add(GeneratePageForModule(assembly));
            }
            
            await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages);
        }

        private Page GeneratePageForModule(Assembly assembly)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Module: " + versionInfo.ProductName,
                Color = DiscordColor.Blue,
                Description = $"Description: {versionInfo.FileDescription}\nVersion: {versionInfo.FileVersion} | {versionInfo.ProductVersion}",
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = versionInfo.CompanyName,
                },
                Timestamp = DateTimeOffset.Now,
            };
            
            return new Page(embed: embed);
        }
    }
}