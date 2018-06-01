using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LolResearchBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LolResearchBot
{
    public class Startup
    {
        private Startup(string[] args)
        {
            var builder = new ConfigurationBuilder(); // Create a new instance of the config builder

            if (File.Exists("/.dockerenv")) // We check if we're running in a Docker container.
                builder.SetBasePath("/data/"); // Yes? Use /data/ mounted volume.
            else
                builder.SetBasePath(AppContext
                    .BaseDirectory); // No? Use the base directory containing the bot's executable.

            builder.AddJsonFile("_configuration.json", false,
                true); // Add this (json encoded) file to the configuration
            Configuration = builder.Build(); // Build the configuration
        }

        private IConfigurationRoot Configuration { get; }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        private async Task RunAsync()
        {
            var services = new ServiceCollection(); // Create a new instance of a service collection
            ConfigureServices(services);

            var provider = services.BuildServiceProvider(); // Build the service provider
            provider.GetRequiredService<LoggingService>(); // Start the logging service
            provider.GetRequiredService<CommandHandlingService>(); // Start the command handler service

            await provider.GetRequiredService<StartupService>().StartAsync(); // Start the startup service
            await Task.Delay(-1); // Keep the program alive
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    // Add discord to the collection
                    LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                    MessageCacheSize = 1000 // Cache 1,000 messages per channel
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    // Add the command service to the collection
                    LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                    DefaultRunMode = RunMode.Async, // Force all commands to run async by default
                    CaseSensitiveCommands = false // Ignore case when executing commands
                }))
                .AddSingleton<StartupService>() // Add startupservice to the collection
                .AddSingleton<LoggingService>() // Add loggingservice to the collection
                .AddSingleton<CommandHandlingService>() // Add commandhandlingservice to the collection
                .AddSingleton<SmmryService>() // Add smmryservice to the collection
                .AddSingleton<ImageService>() // Add imageservice to the collection
                .AddSingleton<LeagueofLegendsService>() // Add leagueoflegends to the collection
                .AddSingleton<HttpClient>() // Add httpclient to the collection
                .AddSingleton<Random>() // Add random to the collection
                .AddSingleton(Configuration); // Add the configuration to the collection
        }
    }
}