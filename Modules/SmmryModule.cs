using System.Threading.Tasks;
using Discord.Commands;
using LolResearchBot.Services;

namespace LolResearchBot.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class SmmryModule : ModuleBase<SocketCommandContext>
    {
        public SmmryService SmmryService { get; set; }

        [Command("summarize")]
        [Alias("sum", "summary")]
        [Summary("Summarizes a given link.")]
        public async Task SummarizeAsync([Remainder] string SM_URL)
        {
            await ReplyAsync(await SmmryService.GetSummaryAsync(SM_URL));
        }
    }
}