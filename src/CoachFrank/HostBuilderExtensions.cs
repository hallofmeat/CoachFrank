using System;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoachFrank;

internal static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureDiscord(this IHostBuilder builder, Action<DiscordConfiguration> configureOptions)
    {
        var config = new DiscordConfiguration();
        configureOptions(config);

        var client = new DiscordClient(config);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(config);
            services.AddSingleton(client);
        });

        return builder;
    }

    public static IHostBuilder ConfigureDiscord(this IHostBuilder builder, Action<HostBuilderContext, DiscordConfiguration> configureOptions)
    {
        builder.ConfigureServices((context, services) =>
        {
            var config = new DiscordConfiguration();
            configureOptions(context, config);

            var client = new DiscordClient(config);

            services.AddSingleton(config);
            services.AddSingleton(client);
        });

        return builder;
    }

    public static IHostBuilder ConfigureDiscordSlashCommands(this IHostBuilder builder)
    {
        var slashCommandsConfig = new SlashCommandsConfiguration();

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(slashCommandsConfig);
        });

        return builder;
    }
}