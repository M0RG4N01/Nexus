﻿using Nexus.Commands;
using Nexus.SDK.Modules;
using Nexus.Utilities;

namespace Nexus
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.CommandsNext.Exceptions;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Enums;
    using Exceptions;
    using SDK.Plugins;
    using Sentry;

    public class DiscordMain
    {
        public static DiscordClient Client;

        public static readonly NexusConfiguration NexusConfiguration =
            new NexusModuleConfiguration<NexusConfiguration>().GetConfiguration();

        public DiscordMain()
        {
            var discordConfig = new DiscordConfiguration
            {
                AutoReconnect = true,
                LargeThreshold = 250,
                LogLevel = LogLevel.Debug,
                Token = NexusConfiguration.DiscordToken,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                MessageCacheSize = 2048,
            };

            Client = new DiscordClient(discordConfig);

            var commandsNextConfiguration = new CommandsNextConfiguration
            {
                StringPrefixes = NexusConfiguration.Prefixes,
                EnableDms = false,
                EnableMentionPrefix = true,
                CaseSensitive = false,
                EnableDefaultHelp = false
            };

            Client.UseInteractivity(
                new InteractivityConfiguration
                {
                    Timeout = TimeSpan.FromMinutes(2),
                    PaginationDeletion = PaginationDeletion.DeleteMessage,
                    PollBehaviour = PollBehaviour.DeleteEmojis
                });

            Client.UseCommandsNext(commandsNextConfiguration);
            var commandsNextService = Client.GetCommandsNext();

            commandsNextService.RegisterCommands<BuiltInCommands>();

            commandsNextService.CommandExecuted += CommandExecutionEvent;
            commandsNextService.CommandErrored += CommandFailed;
        }

        public async Task RunAsync()
        {
            var activity = new DiscordActivity
            {
                Name = NexusConfiguration.PlayingStatus,
                ActivityType = NexusConfiguration.PlayingType
            };
            await Client.ConnectAsync(activity);

            NexusInformation.PluginRepository = new PluginRepository();

            await Task.Delay(0);
        }

        private async Task<bool> HandleException(Exception e, CommandContext ctx)
        {
            if (NexusConfiguration.Diagnostics.DebugMode)
            {
                var logger = new Logger("Debug Exception Handler");
                logger.LogException(e);
            }

            switch (e)
            {
                case InvalidOperationException _:
                case CommandNotFoundException _:
                    return true;
                default:
                {
                    string sentryId = string.Empty;
                    if (NexusConfiguration.Diagnostics.EnableSendingDiagnostics)
                    {
                        sentryId = SentrySdk.CaptureException(e).ToString();
                    }

                    switch (e.Message)
                    {
                        case "Could not find a suitable overload for the command.":
                            return true;
                        case "No matching sub-commands were found, and this group is not executable.":
                            return true;
                        default:
                            switch (e)
                            {
                                case TaskCanceledException _:
                                    return true;
                                case CommandNotFoundException _:
                                    return true;
                                case CustomMessageException customMessage:
                                    customMessage.Context = ctx;

                                    if (!string.IsNullOrEmpty(customMessage.Message))
                                    {
                                        await ctx.RespondAsync(customMessage.Message);
                                    }

                                    if (customMessage.Embed != null)
                                    {
                                        await ctx.RespondAsync(embed: customMessage.Embed);
                                    }

#pragma warning disable 4014
                                    Task.Run(async () => await customMessage.DoAction());
#pragma warning restore 4014

                                    return customMessage.Handled;
                            }

                            if (NexusConfiguration.Diagnostics.EnableSendingDiagnostics)
                            {
                                await ctx.RespondAsync("Exception caught! - **Sentry ID: " + sentryId + "**\n");
                            }

                            return false;
                    }
                }
            }
        }

        private Task CommandExecutionEvent(CommandExecutionEventArgs e)
        {
            return Task.Delay(0);
        }

        private async Task CommandFailed(CommandErrorEventArgs e)
        {
            if (e.Exception?.Message != null && e.Exception.Message.Contains("403"))
            {
                await e.Context.RespondAsync(
                    "**This command required me to have the `Administrator` permission in your discord server. Please allow me this permission and try the command again!**");
                return;
            }

            var exceptionHandled = await HandleException(e.Exception, e.Context);
            if (exceptionHandled) return;

            if (e.Exception is ChecksFailedException cfe)
            {
                foreach (var ex in cfe.FailedChecks)
                    switch (ex)
                    {
                        case CooldownAttribute cooldown:
                            await e.Context.RespondAsync(
                                $"Cooldown: **{cooldown.GetRemainingCooldown(e.Context).Seconds}s**");
                            return;
                    }
            }
        }
    }
}