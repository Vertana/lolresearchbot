using System.Threading.Tasks;
using Discord.Commands;

namespace LolResearchBot.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class PingModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Alias("pong", "hello")]
        public Task PingAsync()
        {
            return ReplyAsync("pong!");
        }
    }
}