using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace LolResearchBot.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            // This value holds the offset where the prefix ends
            var argPos = 0;

            if (message.HasMentionPrefix(_discord.CurrentUser, ref argPos) || IsPrivateMessage(message) || message.HasCharPrefix('!', ref argPos))
            {
                
                var context = new SocketCommandContext(_discord, message);
                var cleanCommand = context.Message.Content.Substring(argPos).Trim();
                using (context.Channel.EnterTypingState())
                {
                    var result = await _commands.ExecuteAsync(context, cleanCommand, _services);

                    if (result.Error.HasValue && result.Error.Value != CommandError.UnknownCommand
                    ) // it's bad practice to send 'unknown command' errors
                        await context.Channel.SendMessageAsync(
                            "There was an error processing the command. Please check spelling, just in case.");
                }
            }
        }

        internal bool IsPrivateMessage(SocketMessage msg)
        {
            return msg.Channel.GetType() == typeof(SocketDMChannel);
        }
    }
}