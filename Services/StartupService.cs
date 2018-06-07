using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace LolResearchBot.Services
{
    public class StartupService
    {
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;
         private LeagueofLegendsService LeagueofLegends { get; }

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            LeagueofLegendsService leagueofLegends)
        {
            _config = config;
            _discord = discord;
            _commands = commands;

            LeagueofLegends = leagueofLegends;
        }

        public async Task StartAsync()
        {
            var discordToken = _config["tokens:discord"]; // Get the discord token from the config file
            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception(
                    "Please enter your bot's token into the `_configuration.json` file found in the application's root directory or the /data/ directory.");

            await _discord.LoginAsync(TokenType.Bot, discordToken); // Login to discord
            await _discord.StartAsync(); // Connect to the websocket

            await _commands.AddModulesAsync(Assembly
                .GetEntryAssembly()); // Load commands and modules into the command service

            await Task.Run(() => LeagueofLegends.CacheAllChampions()).ConfigureAwait(false);//Initialize champion cache at startup
            await Task.Run(() => LeagueofLegends.CacheAllItems()).ConfigureAwait(false);//Initialize item cache at startup
            await Task.Run(() => LeagueofLegends.CacheLeagueVersions()).ConfigureAwait(false);//Initialize league version cache at startup
        }
    }
}