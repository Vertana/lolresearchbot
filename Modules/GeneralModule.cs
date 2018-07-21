using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;


namespace LolResearchBot.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Regex sWhitespace = new Regex(@"\s+");
        private static readonly Regex sDiceRolls = new Regex(@"(?:\d+[dD]\d+|\d+\s*[*+-]\s*\d+[dD]\d+)(?:\s*[*+-]\s*(?:\d+[dD]\d+|\d+))*");

        // Dependency Injection will fill this value in for us
        // [Remainder] takes the rest of the command's arguments as one argument, rather than splitting every space
        [Command("echo")]
        public Task EchoAsync([Remainder] string text)
        // Insert a ZWSP before the text to prevent triggering other bots!
        {
            return ReplyAsync('\u200B' + text);
        }

        [Command("flip")]
        [Alias("coin", "toss", "f")]
        [Summary("Flips a coin.")]
        public Task CoinFlip()
        {
            var rand = new Random();
            if (rand.Next(2) == 1)
            {
                return ReplyAsync("Heads!");
            }
            else
                return ReplyAsync("Tails!");
        }

        [Command("roll")]
        [Alias("dice", "r")]
        [Summary("Rolls some dice. Does basic addition (1d6 + 2d6).")]
        public Task DiceRoll(params string[] diceStrings)
        {
            if (diceStrings.Length == 1)
            {
                int total = 0;
                Regex.Replace(diceStrings[0], @"\s+", "");
                var dice = diceStrings[0].Split('+');
                if (dice.Length == 1)
                {
                    if (sDiceRolls.Match(diceStrings[0]).Success)
                    {
                        var numbers = diceStrings[0].Split('d');
                        var numberofDice = Int32.Parse(numbers[0]);
                        var sizeOfDice = Int32.Parse(numbers[1]);
                        int singleDiceStatementTotal = 0;

                        for (int i = 0; i < numberofDice; i++)
                        {
                            var rand = new Random().Next(1, sizeOfDice);
                            singleDiceStatementTotal += rand;
                        }
                        total += singleDiceStatementTotal;
                        return ReplyAsync($"The total is {total}!");

                    }
                    else
                        return ReplyAsync("That's not proper dice!");
                }
                else
                {
                    foreach (string diceString in dice)
                    {
                        if (diceString == "+")
                            continue;
                        var numbers = diceString.Split('d');
                        var numberofDice = Int32.Parse(numbers[0]);
                        var sizeOfDice = Int32.Parse(numbers[1]);
                        int singleDiceStatementTotal = 0;

                        for (int i = 0; i < numberofDice; i++)
                        {
                            var rand = new Random().Next(1, sizeOfDice);
                            singleDiceStatementTotal += rand;
                        }
                        total += singleDiceStatementTotal;
                    }
                    return ReplyAsync($"The total is {total}!");
                }

            }
            else
            {
                int total = 0;
                foreach (string diceString in diceStrings)
                {
                    if (diceString == "+")
                        continue;
                    var cleanString = diceString.Replace("+", string.Empty);
                    var numbers = cleanString.Split('d');
                    var numberofDice = Int32.Parse(numbers[0]);
                    var sizeOfDice = Int32.Parse(numbers[1]);
                    int singleDiceStatementTotal = 0;

                    for (int i = 0; i < numberofDice; i++)
                    {
                        var rand = new Random().Next(1, sizeOfDice);
                        singleDiceStatementTotal += rand;
                    }
                    total += singleDiceStatementTotal;
                }
                return ReplyAsync($"The total is {total}!");
            }

        }
    }
}