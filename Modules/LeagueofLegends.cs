using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LolResearchBot.Model.Extensions;
using LolResearchBot.Services;
using RiotSharp.SummonerEndpoint;

namespace LolResearchBot.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class LeagueofLegendsModule : ModuleBase<SocketCommandContext>
    {
        public LeagueofLegendsModule(LeagueofLegendsService leagueofLegends, ImageService imageService)
        {
            LeagueofLegends = leagueofLegends;
            ImageService = imageService;
        }

        private LeagueofLegendsService LeagueofLegends { get; }
        private ImageService ImageService { get; }

        [Command("stats")]
        [Alias("stat")]
        [Summary("Get League of Legends stats for a given Summoner name..")]
        public async Task GetSummonerStatsAsync(params string[] summonerName)
        {
            foreach (var summonerRef in summonerName)
            {
                var summonerRefNormalized =
                    FirstLetterToUpper(summonerRef); // We normalize the name to account for any capitlizations
                var summoner = await Task.Run(() => LeagueofLegends.GetSummonerAsync(summonerRefNormalized));
                //var champ = await Task.Run(() => LeagueofLegends.GetSummonerLeagueStatsAsync(summoner));
                if (summoner != null) //&& champ != null)
                {
                    var id = summoner.Id;
                    var level = summoner.Level;
                    await ReplyAsync(
                        Format.Code(
                            $@"Summoner: {summoner.Name}{Environment.NewLine}Summer id: {id.ToString()}{
                                    Environment.NewLine
                                }Summoner level: {level.ToString()}"));
                }
                else
                {
                    await ReplyAsync($"Could not retrieve info about {summonerRef}.");
                }
            }
        }

        [Command("games")]
        [Alias("history")]
        [Summary("Get League of Legends recent matches for a given Summoner name.")]
        public async Task GetRecentMatchesAsync([Remainder] string summonerName)
        {
            summonerName = FirstLetterToUpper(summonerName); // We normalize the name to account for any capitlizations
            var matches = await Task.Run(() => LeagueofLegends.GetRecentGamesAsync(summonerName));
            if (matches != null)
            {
                var counter = 0;
                var recentGamesString = "";
                foreach (var match in matches.Matches)
                {
                    counter++;
                    var champ = await Task.Run(() => LeagueofLegends.GetChampionInfoAsync((int) match.ChampionID))
                        .ConfigureAwait(false);
                    var matchInfo = await Task.Run(() => LeagueofLegends.GetMatchInfo(match.GameId));
                    var time = matchInfo.GameDuration;
                    var matchDetails = LeagueofLegends.GetMatchInfo(matchInfo.GameId);
                    var result = matchDetails.Result;
                    recentGamesString +=
                        Format.Code(
                            $"Game: {counter}{Environment.NewLine}Game ID: {match.GameId}{Environment.NewLine}Game Mode: {matchInfo.GameMode}{Environment.NewLine}Lane: {match.Lane.ToString()}{Environment.NewLine}Role: {match.Role}{Environment.NewLine}Season: {FormatSeason(matchInfo.SeasonId)}{Environment.NewLine}Champion: {champ.Name}{Environment.NewLine}Game Length: {ConvertTime(time)}") +
                        Environment.NewLine;
                }

                await ReplyAsync($"Last 5 games:{Environment.NewLine}" + recentGamesString);
            }
            else
            {
                await ReplyAsync("There was an error retrieving recent games. The error has been logged.");
            }
        }

        [Command("detail")]
        [Alias("details", "dt")]
        [Summary("Get detailed info about a match given a summoner name..")]
        public async Task GetDetailedMatchInfoAsync([Remainder] string summonerName)
        {
            var normalizedSummonerName = FirstLetterToUpper(summonerName);
            try
            {
                var match = await Task.Run(() => LeagueofLegends.GetMostRecentGame(normalizedSummonerName));
                await GetMatchInfoAsync(match.GameId);
            }
            catch (Exception)
            {
                await ReplyAsync(
                    "There was an error generating the image or getting the recent match. The error was logged.");
            }
        }

        [Command("match")]
        [Alias("game")]
        [Summary("Get detailed info about a match.")]
        public async Task GetMatchInfoAsync([Remainder] long gameId)
        {
            var match = await Task.Run(() => LeagueofLegends.GetMatchInfo(gameId));
            var matchEx = new MatchExtend(match);
            var header = string.Format("{0,-25} {1,-15} {2,-8} {3,-15} {4,-13} {5,-15} {6,-15}", "Name:",
                "Highest Tier:", "Lane:", "Role:", "KDA:", "Damage Dealt:", "Damage Taken:");
            var playerStrings = new List<string>();
            var titles = new List<string>();

            if (match != null)
            {
                foreach (var team in matchEx.Teams)
                {
                    var didTeamWin = team.Stats.Win;
                    var towerKills = team.Stats.TowerKills;
                    var firstBlood = team.Stats.FirstBlood;

                    foreach (var person in team.Participants)
                    {
                        var name = person.SummonerName;
                        var highestTier = person.Participant.HighestAchievedSeasonTier;
                        var timeLine = person.Participant.Timeline;
                        var matchRef = await Task.Run(() =>
                            LeagueofLegends.GetSpecificMatchReferenceAsync(person.ParticipantIdentity.Player.AccountId,
                                match.GameId));
                        var lane = matchRef.Lane;
                        var role = matchRef.Role;
                        var assists = person.Participant.Stats.Assists;
                        var kills = person.Participant.Stats.Kills;
                        var deaths = person.Participant.Stats.Deaths;
                        var damageDealt = string.Format("{0:n0}", person.Participant.Stats.TotalDamageDealtToChampions);
                        var damageTaken = string.Format("{0:n0}", person.Participant.Stats.TotalDamageTaken);
                        var healed = person.Participant.Stats.TotalHeal;
                        //var champ = await Task.Run(() => LeagueofLegends.GetChampionInfo((int) person.ChampionId));

                        playerStrings.Add(string.Format("{0,-25} {1,-15} {2,-8} {3,-15} {4,-13} {5,-15} {6,-15} {7}",
                            name, highestTier, lane.ToString(), role.ToString(), assists + "/" + kills + "/" + deaths,
                            damageDealt, damageTaken, Environment.NewLine));
                    }

                    titles.Add(
                        $"Team {team.Stats.TeamId / 100}  Outcome: {(didTeamWin == "Win" ? "Victory" : "Defeat")}    Towers Killed: {towerKills}    First Blood? {(firstBlood ? "Yes" : "No")}");
                }

                string team1;
                string team2;

                if (playerStrings.Count == 10)
                {
                    team1 = string.Join(string.Empty, playerStrings.GetRange(0, 5));
                    team2 = string.Join(string.Empty, playerStrings.GetRange(5, 5));
                }

                else if (playerStrings.Count == 6)
                {
                    team1 = string.Join(string.Empty, playerStrings.GetRange(0, 3));
                    team2 = string.Join(string.Empty, playerStrings.GetRange(3, 3));
                }
                else
                {
                    team1 = string.Join(string.Empty, playerStrings.GetRange(0, 5));
                    team2 = string.Join(string.Empty, playerStrings.GetRange(5, 5));
                }

                var team1Header = titles[0] + Environment.NewLine + Environment.NewLine + header + Environment.NewLine;
                var team2Header = titles[1] + Environment.NewLine + Environment.NewLine + header + Environment.NewLine;
                var body = team1Header + team1 + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                           team2Header + team2;

                try
                {
                    var fileName = $"{matchEx.Match.GameId.ToString()}";
                    var imageLocation = ImageService.CreateTextImage(body, fileName);
                    await Context.Channel.SendFileAsync(imageLocation);
                }
                catch (Exception)
                {
                    await ReplyAsync("There was an error generating the image for the data. The error was logged.");
                }
            }
            else
            {
                await ReplyAsync("There was an error retrieving recent games. The error has been logged.");
            }
        }

        // If the following command gives an API limitation error, it is probably the line the line calling GetChampionInfo which uses the RiotStaticApi. The rate limiting isn't as good on the Static API Endpoint.
        [Command("recent")]
        [Alias("shame")]
        [Summary("Get the most recent game for a summoner")]
        public async Task GetMostRecentMatchAsync([Remainder] string summonerName)
        {
            var normalizedSummonerName = FirstLetterToUpper(summonerName);
            MatchExtend matchEx;
            Summoner summoner;
            long accountId;
            try
            {
                var match = await Task.Run(() => LeagueofLegends.GetMostRecentGame(normalizedSummonerName));
                matchEx = new MatchExtend(match);
                summoner = await Task.Run(() => LeagueofLegends.GetSummonerAsync(normalizedSummonerName));
                accountId = summoner.AccountId;
            }
            catch (Exception)
            {
                await ReplyAsync(
                    $"There was an error retrieving recent match for {summonerName}. The error has been logged.");
                return;
            }


            if (matchEx != null)
            {
                var shamefulString = "";
                var worstPlayerString = "";
                var reasonWeLost = "";

                foreach (var participant in matchEx.Match.ParticipantIdentities)
                foreach (var person in matchEx.Match.Participants)
                    if (person.ParticipantId == participant.ParticipantId && participant.Player.AccountId == accountId)
                    {
                        var name = participant.Player.SummonerName;
                        var highestTier = person.HighestAchievedSeasonTier;
                        var lane = person.Timeline.Lane;
                        var role = person.Timeline.Role;
                        var assists = person.Stats.Assists;
                        var kills = person.Stats.Kills;
                        var deaths = person.Stats.Deaths;
                        var damageDealt = person.Stats.TotalDamageDealtToChampions;
                        var damageTaken = person.Stats.TotalDamageTaken;
                        var healed = person.Stats.TotalHeal;
                        var champ = await Task.Run(() => LeagueofLegends.GetChampionInfoAsync(person.ChampionId));
                        var time = matchEx.Match.GameDuration;
                        var outcome = person.Stats.Winner;
                        var champLevel = person.Stats.ChampLevel;

                        var playerTeam = matchEx.Teams.First(x => x.Stats.TeamId == person.TeamId);

                        worstPlayerString =
                            $"{playerTeam.WorstParticipant.SummonerName} was the worst player on the team.";

                        shamefulString +=
                            $"Game ID: {matchEx.Match.GameId}{Environment.NewLine}Name: {name}{Environment.NewLine}Game Mode: {matchEx.Match.GameMode}{Environment.NewLine}Lane: {lane}{Environment.NewLine}Role: {role}{Environment.NewLine}Champion: {champ.Name}{Environment.NewLine}Champ Level: {champLevel}{Environment.NewLine}Game Length: {ConvertTime(time)}{Environment.NewLine}KDA: {kills}/{deaths}/{assists}{Environment.NewLine}Highest Rank (at the end of a season): {highestTier}{Environment.NewLine}Damage Dealt: {damageDealt.ToString("N0")}{Environment.NewLine}Damage Taken: {damageTaken.ToString("N0")}";

                        //!outcome = the player lost their game
                        if (damageTaken > damageDealt && !outcome && deaths >= kills + 3 && !role.Contains("SUPPORT"))
                            reasonWeLost = $"{name} is the reason we lost.";
                        else if (healed < damageTaken && !outcome && deaths >= kills + 6 && assists <= kills + deaths &&
                                 role.Contains("SUPPORT")) reasonWeLost = $"{name} is the reason we lost.";
                    }

                await ReplyAsync($"Most recent game:{Environment.NewLine}" + Format.Code(shamefulString) +
                                 Environment.NewLine + reasonWeLost + Environment.NewLine + worstPlayerString);
            }
            else
            {
                await ReplyAsync("There was an error retrieving recent games. The error has been logged.");
            }
        }

        // Converting the time is only to display the Match Duration. This is to work around a bug where RiotSharp 3.0.1 treats LoL seconds like milliseconds.
        private static string ConvertTime(TimeSpan duration)
        {
            var durationMinutes = duration.TotalMilliseconds / 60;
            var intPart = (long) durationMinutes;
            var fractionalPart = durationMinutes - intPart;
            var durationSeconds =
                fractionalPart / 100 * 60 *
                100; // Converts percentage to raw seconds out of 60. (such as converting 93% to 55 seconds)
            return $"{((int) durationMinutes).ToString()}:{((int) durationSeconds).ToString("00")}";
        }

        // This just converts the season returned (in int form) from LoL API to English.
        private static string FormatSeason(int season)
        {
            switch (season)
            {
                case 0:
                    return "Preseason 3";
                case 1:
                    return "Season 3";
                case 2:
                    return "Preseason 2014";
                case 3:
                    return "Season 2014";
                case 4:
                    return "Preseason 2015";
                case 5:
                    return "Season 2015";
                case 6:
                    return "Preseason 2016";
                case 7:
                    return "Season 2016";
                case 8:
                    return "Preseason 2017";
                case 9:
                    return "Season 2017";
                case 10:
                    return "Preseason 2018";
                case 11:
                    return "Season 2018";
                default:
                    return "Unknown Season";
            }
        }

        // How we normalize summoner names to have first letter capitalized, all others lower case as that's what LoL API accepts without any issues.
        private string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }
}