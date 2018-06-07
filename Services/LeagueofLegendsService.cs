using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using RiotSharp;
using RiotSharp.LeagueEndpoint;
using RiotSharp.MatchEndpoint;
using RiotSharp.Misc;
using RiotSharp.SpectatorEndpoint;
using RiotSharp.StaticDataEndpoint.Champion;
using RiotSharp.SummonerEndpoint;

namespace LolResearchBot.Services
{
    public class LeagueofLegendsService
    {
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        private readonly DiscordSocketClient _discord;
        private readonly LoggingService _logging;
        private readonly LeagueFileCacheService _fileCache;
        private readonly TimeSpan cacheTimer;

        private readonly RiotApi api;
        private readonly StaticRiotApi staticApi;

        public static string latestVersion;

        public LeagueofLegendsService(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            LoggingService logging,
            LeagueFileCacheService fileCache)
        {
            _config = config;
            _discord = discord;
            _commands = commands;
            _logging = logging;
            _fileCache = fileCache;

            try
            {
                cacheTimer = TimeSpan.FromHours(1);
                api = RiotApi.GetInstance(RiotApiKey, RateLimitPer10S,
                    RateLimitPer10M); // This gets an API instance with our API Key and rate limits.
                staticApi = StaticRiotApi.GetInstance(RiotApiKey, true,
                    cacheTimer); // THis gets a static endpoint API instance with our API key.
            }
            catch (RiotSharpException ex)
            {
                _logging.OnLogAsync(ex).ConfigureAwait(false);
            }
        }

        /*
    Set the rate limit for Riot API per 10 seconds and per 10 minutes in the config file in the following section:
        "LeagueofLegendsOptions": 
        {
            "RateLimitPer10S": "X",
            "RateLimitPer10M": "X"
        } 
    */
        private int _RateLimitPer10S { get; set; }

        private int RateLimitPer10S
        {
            get
            {
                _RateLimitPer10S = Convert.ToInt32(_config["LeagueofLegendsOptions:RateLimitPer10S"]);
                return _RateLimitPer10S;
            }
        }

        private int _RateLimitPer10M { get; set; }

        private int RateLimitPer10M
        {
            get
            {
                _RateLimitPer10M = Convert.ToInt32(_config["LeagueofLegendsOptions:RateLimitPer10M"]);
                return _RateLimitPer10M;
            }
        }

        private string _RiotApiKey { get; set; }

        private string RiotApiKey
        {
            get
            {
                _RiotApiKey = _config["tokens:LeagueofLegends"] ?? "Invalid";
                return _RiotApiKey;
            }
        }

        public async Task<Summoner> GetSummonerAsync(string summonerName)
        {
            try
            {
                return await Task.Run(() => api.GetSummonerByNameAsync(Region.na, summonerName)).ConfigureAwait(false);
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return null;
            }
        }

        public async Task<MatchReference> GetSpecificMatchReferenceAsync(long accountId, long matchId)
        {
            try
            {
                var matchList = await Task.Run(() => api.GetMatchListAsync(Region.na, accountId)).ConfigureAwait(false);
                var match = matchList.Matches.Find(x => x.GameId == matchId);
                return match;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return null;
            }
        }


        public async Task<List<League>> GetSummonerLeagueStatsAsync(Summoner summoner)
        {
            try
            {
                var league = await Task.Run(() => api.GetLeaguesAsync(Region.na, summoner.AccountId))
                    .ConfigureAwait(false);
                return league;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return null;
            }
        }

        public async Task<MatchList> GetRecentGamesAsync(string summonerName)
        {
            try
            {
                var summoner = api.GetSummonerByName(Region.na, summonerName);
                var matches = await Task.Run(() =>
                        api.GetMatchListAsync(summoner.Region, summoner.AccountId, beginIndex: 0, endIndex: 5))
                    .ConfigureAwait(false);
                return matches;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return null;
            }
        }

        public async Task<Match> GetMostRecentGame(string summonerName)
        {
            try
            {
                var summoner = api.GetSummonerByName(Region.na, summonerName);
                var matches = await Task.Run(() =>
                        api.GetMatchListAsync(summoner.Region, summoner.AccountId, beginIndex: 0, endIndex: 1))
                    .ConfigureAwait(false);
                var firstMatch = matches.Matches[0];
                var match = await Task.Run(() => api.GetMatchAsync(Region.na, firstMatch.GameId)).ConfigureAwait(false);
                return match;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return null;
            }
        }

        public async Task<CurrentGame> GetCurrentGameInfo(int gameId)
        {
            try
            {
                var currentGame =
                    await Task.Run(() => api.GetCurrentGameAsync(Region.na, gameId)).ConfigureAwait(false);
                return currentGame;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return null;
            }
        }

        public async Task<ChampionStatic> GetChampionInfoAsync(int champId)
        {
            try
            {
                var champ = await Task.Run(() => staticApi.GetChampionAsync(Region.na, champId)).ConfigureAwait(false);
                return champ;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return null;
            }
        }

        public async Task<Match> GetMatchInfo(long matchId)
        {
            try
            {
                var match = await Task.Run(() => api.GetMatch(Region.na, matchId)).ConfigureAwait(false);
                return match;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return null;
            }
        }
        public async Task CacheAllChampions()
        {
            try
            {
                var champs = await Task.Run(() => staticApi.GetChampionsAsync(Region.na, championData: RiotSharp.StaticDataEndpoint.ChampionData.All));
                var result = _fileCache.CreateChampionCache(champs);
                return;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return;
            }
        }

        public async Task CacheAllItems()
        {
            try
            {
                var items = await Task.Run(() => staticApi.GetItemsAsync(Region.na, itemData: RiotSharp.StaticDataEndpoint.ItemData.All));
                var result = _fileCache.CreateItemCache(items);
                return;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return;
            }
        }

        public async Task CacheLeagueVersions()
        {
            try
            {
                var versionList =  await Task.Run(() => staticApi.GetVersionsAsync(Region.na));
                await _fileCache.CreateVersionCache(versionList);
                return;
            }
            catch (RiotSharpException ex)
            {
                // Handle the exception however you want.
                await _logging.OnLogAsync(ex).ConfigureAwait(false);
                return;
            }
        }
    }
}