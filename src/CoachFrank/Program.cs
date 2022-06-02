using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CoachFrank.Data;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace CoachFrank
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateHostBuilder(args).Build();
                InitializeDatabase(host);
                host.Run();
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging((context, loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    loggingBuilder.AddNLog(context.Configuration);
                })
                .ConfigureContainer<ContainerBuilder>(builder => 
                {
                    builder.RegisterModule(new CoachFrankRegistry());
                })
                .ConfigureDiscord((context, options) =>
                {
                    var botSettings = context.Configuration.GetSection("BotSettings").Get<BotSettings>();

                    options.LoggerFactory = new NLogLoggerFactory();
                    options.TokenType = TokenType.Bot;
                    options.Token = botSettings.DiscordToken;
                    options.Intents = DiscordIntents.AllUnprivileged;
                    options.AutoReconnect = true;
                })
                .ConfigureDiscordSlashCommands()
                .ConfigureServices((context, services) =>
                {
                    services.Configure<BotSettings>(context.Configuration.GetSection("BotSettings"));
                    services.AddDbContext<BotContext>(options => options.UseSqlite(context.Configuration.GetConnectionString(nameof(BotContext))));

                    services.AddHostedService<DiscordBotService>();
                });
        }

        private static void InitializeDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<BotContext>().Database.Migrate();
        }

        //For creating migrations
        public class BotContextFactory : IDesignTimeDbContextFactory<BotContext>
        {
            public BotContext CreateDbContext(string[] args)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();

                var options = new DbContextOptionsBuilder<BotContext>();
                options.UseSqlite(config.GetConnectionString(nameof(BotContext)));

                return new BotContext(options.Options);
            }
        }
    }
}

