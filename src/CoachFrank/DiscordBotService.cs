using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CoachFrank.Commands;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog;

namespace CoachFrank
{
    internal sealed class DiscordBotService : IHostedService
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DiscordClient _client;
        private readonly SlashCommandsConfiguration _slashCommandsConfig;
        private readonly IOptionsSnapshot<BotSettings> _botSettings;

        public DiscordBotService(DiscordClient client, SlashCommandsConfiguration slashCommandsConfig, IOptionsSnapshot<BotSettings> botSettings, IServiceProvider serviceProvider)
        {
            _client = client;
            _slashCommandsConfig = slashCommandsConfig;
            _botSettings = botSettings;
            _slashCommandsConfig.Services = serviceProvider; //Force IOC container
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            var commandAssembly = typeof(UtilCommands).GetTypeInfo().Assembly;

            var slashCommands = _client.UseSlashCommands(_slashCommandsConfig);
            slashCommands.RegisterCommands(commandAssembly, _botSettings.Value?.GuildId);

            slashCommands.SlashCommandExecuted += LogExecutedSlashCommand;
            slashCommands.SlashCommandErrored += LogErrorSlashCommand;

            Logger.Info("Connecting Discord client");

            await _client.ConnectAsync();

            Logger.Info("Connected Discord client");

        }



        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.Info("Stopping Discord client");

            using (_client)
            {
                await _client.DisconnectAsync();
            }

            Logger.Info("Stopped Discord Client");
        }

        private Task LogErrorSlashCommand(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            Logger.Error(e.Exception,
                $"An exception was thrown while executing Command:'{e?.Context?.CommandName}' User:'{e?.Context?.User}'");

            return Task.CompletedTask;
        }

        private Task LogExecutedSlashCommand(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            var guildName = e?.Context?.Guild?.Name ?? "Direct Message";
            Logger.Info($"Executed Command:'{e?.Context?.CommandName}' User:'{e?.Context?.User}' in '{guildName}'");

            return Task.CompletedTask;
        }
    }
}