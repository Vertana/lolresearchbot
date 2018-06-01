using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

//Example taken from https://github.com/Aux/Discord.Net-Example/blob/1.0/src/Services/LoggingService.cs
namespace LolResearchBot.Services
{
    public class LoggingService
    {
        private readonly CommandHandlingService _commandHandling;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;

        // DiscordSocketClient and CommandService are injected automatically from the IServiceProvider
        public LoggingService(DiscordSocketClient discord, CommandService commands,
            CommandHandlingService commandHandling)
        {
            if (File.Exists("/.dockerenv")) // We check if we're running in a Docker container.
                _logDirectory = Path.Combine("/data/", "logs"); // Yes? Use /data/ mounted volume.
            else
                _logDirectory =
                    Path.Combine(AppContext.BaseDirectory,
                        "logs"); // No? Use the base directory containing the bot's executable.


            _discord = discord;
            _commands = commands;
            _commandHandling = commandHandling;

            _discord.Log += OnLogAsync;
            _discord.MessageReceived += OnLogAsync;
            _commands.Log += OnLogAsync;
        }

        private string _logDirectory { get; }
        private string _logFile => Path.Combine(_logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

        internal Task OnLogAsync(LogMessage msg)
        {
            if (!Directory.Exists(_logDirectory)) // Create the log directory if it doesn't exist
                Directory.CreateDirectory(_logDirectory);
            if (!File.Exists(_logFile)) // Create today's log file if it doesn't exist
                File.Create(_logFile).Dispose();

            var logText =
                $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            File.AppendAllText(_logFile, logText + "\n"); // Write the log text to a file

            var consoleText =
                $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{msg.Severity}] {msg.Source}: {msg.Message ?? msg.Exception.ToString()}";

            return Console.Out.WriteLineAsync(consoleText); // Write the log text to the console
        }

        // This is an overloaded function to log the exact user input (on msgs aimed at bot). Hooked into MessageReceived event.
        internal Task OnLogAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage msg)) return Task.CompletedTask;
            if (msg.Source != MessageSource.User) return Task.CompletedTask;
            // This value holds the offset where the prefix ends
            var argPos = 0;
            if (msg.HasMentionPrefix(_discord.CurrentUser, ref argPos) || _commandHandling.IsPrivateMessage(msg))
            {
                if (!Directory.Exists(_logDirectory)) // Create the log directory if it doesn't exist
                    Directory.CreateDirectory(_logDirectory);
                if (!File.Exists(_logFile)) // Create today's log file if it doesn't exist
                    File.Create(_logFile).Dispose();

                var logText = $"{DateTime.UtcNow.ToString("hh:mm:ss")} [Message Received] {msg.Author}: {msg.Content}";
                File.AppendAllText(_logFile, logText + "\n"); // Write the log text to a file

                return Console.Out.WriteLineAsync(logText); // Write the log text to the console
            }

            return Task.CompletedTask;
        }

        internal Task OnLogAsync(Exception ex)
        {
            if (!Directory.Exists(_logDirectory)) // Create the log directory if it doesn't exist
                Directory.CreateDirectory(_logDirectory);
            if (!File.Exists(_logFile)) // Create today's log file if it doesn't exist
                File.Create(_logFile).Dispose();

            var msg = new LogMessage(LogSeverity.Error, ex.Source, ex.Message, ex);

            var logText =
                $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{msg.Severity}] {msg.Source}: {msg.Message ?? msg.Exception.ToString()}";
            File.AppendAllText(_logFile, logText + "\n"); // Write the log text to a file

            return Console.Out.WriteLineAsync(logText); // Write the log text to the console
        }
    }
}