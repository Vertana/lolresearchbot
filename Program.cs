using System.Threading.Tasks;

namespace LolResearchBot
{
    internal class Program
    {
        public static Task Main(string[] args)
        {
            return Startup.RunAsync(args);
        }
    }
}