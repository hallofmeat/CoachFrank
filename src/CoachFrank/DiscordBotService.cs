using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CoachFrank.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Hosting;
using NLog;

namespace CoachFrank
{
    internal sealed class DiscordBotService : IHostedService
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DiscordClient _client;
        private readonly CommandsNextConfiguration _commandsConfig;

        public DiscordBotService(DiscordClient client, CommandsNextConfiguration commandsConfig, IServiceProvider serviceProvider)
        {
            _client = client;
            _commandsConfig = commandsConfig;
            _commandsConfig.Services = serviceProvider; //Force IOC container
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var commandsNext = _client.UseCommandsNext(_commandsConfig);

            var commandAssembly = typeof(UtilCommands).GetTypeInfo().Assembly;
            commandsNext.RegisterCommands(commandAssembly);

            commandsNext.CommandExecuted += LogExecutedCommand;
            commandsNext.CommandErrored += LogErrorCommand;

            Logger.Info($"Registered {commandsNext.RegisteredCommands.Count} commands");

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

        private Task LogErrorCommand(CommandsNextExtension context, CommandErrorEventArgs e)
        {
            Logger.Error(e.Exception,
                $"An exception was thrown while executing Command:'{e?.Command?.Name}' User:'{e?.Context?.User}'");

            return Task.CompletedTask;
        }

        private Task LogExecutedCommand(CommandsNextExtension context, CommandExecutionEventArgs e)
        {
            var guildName = e?.Context?.Guild?.Name ?? "Direct Message";
            Logger.Info($"Executed Command:'{e?.Command?.Name}' User:'{e?.Context?.User}' in '{guildName}'");

            return Task.CompletedTask;
        }
    }
}