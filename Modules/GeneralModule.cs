using System.Threading.Tasks;
using Discord.Commands;

namespace LolResearchBot.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        public Task EchoAsync([Remainder] string text)
            // Insert a ZWSP before the text to prevent triggering other bots!
        {
            return ReplyAsync('\u200B' + text);
        }

        // 'params' will parse space-separated elements into a list
        [Command("list")]
        public Task ListAsync(params string[] objects)
        {
            return ReplyAsync("You listed: " + string.Join("; ", objects));
        }
    }
}