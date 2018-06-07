using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LolResearchBot.Model;
using Microsoft.Extensions.Configuration;
using RiotSharp;
using ProtoBuf;

namespace LolResearchBot.Services
{
    public class LeagueFileCacheService
    {
        private readonly LoggingService _logging;
        private readonly IConfigurationRoot _config;
        private readonly SystemFileService _fileService;
        private static readonly Regex sWhitespace = new Regex(@"\s+");

        private int _cacheSize { get; set; }

        private int cacheSize
        {
            get
            {
                _cacheSize = Convert.ToInt32(_config["systemOptions:leagueCacheSize"]);
                return _cacheSize;
            }
        }

        private string _leagueCacheFolder { get; set; }

        public string leagueCacheFolder
        {
            get
            {
                if (File.Exists("/.dockerenv")) // We check if we're running in a Docker container.
                {
                    _leagueCacheFolder = "/data/league/"; // Yes? Use /data/ mounted volume.
                    return _leagueCacheFolder;
                }

                _leagueCacheFolder = _config["systemOptions:leagueCacheFolder"];
                return _leagueCacheFolder;
            }
        }
        private static string _leagueChampionCacheFolder { get; set; }

        public string leagueChampionCacheFolder
        {
            get
            {
                if (File.Exists("/.dockerenv")) // We check if we're running in a Docker container.
                {
                    _leagueChampionCacheFolder = "/data/league/champs"; // Yes? Use /data/ mounted volume.
                    return _leagueChampionCacheFolder;
                }

                _leagueChampionCacheFolder = Path.Combine(leagueCacheFolder, "champs/");
                return _leagueChampionCacheFolder;
            }
        }

        private string _leagueItemCacheFolder { get; set; }

        public string leagueItemCacheFolder
        {
            get
            {
                if (File.Exists("/.dockerenv")) // We check if we're running in a Docker container.
                {
                    _leagueItemCacheFolder = "/data/league/items"; // Yes? Use /data/ mounted volume.
                    return _leagueItemCacheFolder;
                }

                _leagueItemCacheFolder = Path.Combine(leagueCacheFolder, "items/");
                return _leagueItemCacheFolder;
            }
        }

        private static ConcurrentDictionary<int, string> champDict = new ConcurrentDictionary<int, string>();
        public LookupDictionary champIndex = new LookupDictionary();

        private static ConcurrentDictionary<int, string> itemDict = new ConcurrentDictionary<int, string>();
        public LookupDictionary itemIndex = new LookupDictionary();
        private static ConcurrentDictionary<int, string> verDict = new ConcurrentDictionary<int, string>();
        public LookupDictionary verIndex = new LookupDictionary();

        public LeagueFileCacheService(IConfigurationRoot config, LoggingService logging, SystemFileService fileService)
        {
            _config = config;
            _logging = logging;
            _fileService = fileService;

            if (Directory.Exists(leagueCacheFolder))
                _fileService.ClearCache(leagueCacheFolder, cacheSize);
            else
                Directory.CreateDirectory(leagueCacheFolder);

            if (!Directory.Exists(leagueChampionCacheFolder))
                Directory.CreateDirectory(leagueChampionCacheFolder);

            if (!Directory.Exists(leagueItemCacheFolder))
                Directory.CreateDirectory(leagueItemCacheFolder);

            if (File.Exists(Path.Combine(leagueChampionCacheFolder, "index.cache")))
            {
                champIndex = LoadIndexFile(Path.Combine(leagueChampionCacheFolder, "index.cache"));
                champDict = champIndex.Index;
            }

            if (File.Exists(Path.Combine(leagueItemCacheFolder, "index.cache")))
            {
                itemIndex = LoadIndexFile(Path.Combine(leagueItemCacheFolder, "index.cache"));
                itemDict = itemIndex.Index;
            }

            if (File.Exists(Path.Combine(leagueCacheFolder, "versions.cache")))
            {
                verIndex = LoadIndexFile(Path.Combine(leagueCacheFolder, "versions.cache"));
                verDict = verIndex.Index;
            }
        }

        private LookupDictionary LoadIndexFile(string filePath)
        {
            if (File.Exists(filePath)) // Does the index file exist?
            {
                LookupDictionary dictionary = new LookupDictionary();
                using (var file = File.OpenRead(filePath))
                {
                    dictionary = Serializer.Deserialize<LookupDictionary>(file); // Deserialize our lookup into the file
                }
                return dictionary;
            }
            else
                return null;
        }

        #region champions
        public Task CreateChampionCache(RiotSharp.StaticDataEndpoint.Champion.ChampionListStatic champs)
        {
            Parallel.ForEach(champs.Champions.Values, (champ) =>
            {
                var champion = new LeagueFileCache.Champion
                {
                    Id = champ.Id,
                    Name = champ.Name,
                    Lore = champ.Lore,
                    Stats = new LeagueFileCache.ChampStats
                    {
                        armorPerLevel = champ.Stats.ArmorPerLevel,
                        hpPerLevel = champ.Stats.HpPerLevel,
                        mpPerLevel = champ.Stats.MpPerLevel,
                        armor = champ.Stats.Armor,
                        hp = champ.Stats.Hp,
                        critPerLevel = champ.Stats.CritPerLevel
                    },
                    Passive = new LeagueFileCache.Passive
                    {
                        sanitizedDescription = champ.Passive.SanitizedDescription,
                        Name = champ.Passive.Name,
                        Description = champ.Passive.Description
                    },
                    Info = new LeagueFileCache.Info
                    {
                        Attack = champ.Info.Attack,
                        Defense = champ.Info.Defense,
                        Difficulty = champ.Info.Difficulty,
                        Magic = champ.Info.Magic,
                    },
                    Image = new LeagueFileCache.Image
                    {
                        Full = champ.Image.Full,
                        Group = champ.Image.Group,
                        Sprite = champ.Image.Sprite,
                        h = champ.Image.Height,
                        w = champ.Image.Width,
                        x = champ.Image.X,
                        y = champ.Image.Y
                    }
                };
                var name = normalizeNames(champ.Name);
                var fileName = Path.Combine(leagueChampionCacheFolder, name + "." + champ.Id + ".cache");
                using (var file = File.Create(fileName))
                {
                    Serializer.Serialize(file, champion);
                }
                champDict.TryAdd(champ.Id, name);
            });

            var indexFileName = Path.Combine(leagueChampionCacheFolder, "index.cache");
            champIndex.Name = "Champions";
            champIndex.Index = Serializer.DeepClone(champDict);
            using (var file = File.Create(indexFileName))
            {
                Serializer.Serialize(file, champIndex);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region items

        public Task CreateItemCache(RiotSharp.StaticDataEndpoint.Item.ItemListStatic items)
        {
            Parallel.ForEach(items.Items.Values, (item) =>
            {
                var serializedItem = new LeagueFileCache.Item
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    sanitizedDescription = item.SanitizedDescription,
                    BasePrice = item.Gold.BasePrice,
                    SellingPrice = item.Gold.SellingPrice,
                    TotalPrice = item.Gold.TotalPrice,
                    BuiltFrom = item.From,
                    Stats = new LeagueFileCache.ItemStats
                    {
                        PercentMovementSpeedMod = item.Stats.PercentMovementSpeedMod,
                        PercentLifeStealMod = item.Stats.PercentLifeStealMod,
                        PercentAttackSpeedMod = item.Stats.PercentAttackSpeedMod,
                        PercentArmorMod = item.Stats.PercentArmorPenetrationMod,
                        PercentCooldownMod = item.Stats.PercentCooldownMod,
                        PercentCritChanceMod = item.Stats.PercentCritChanceMod,
                        PercentCritDamageMod = item.Stats.PercentCritDamageMod,
                        PercentHPRegenMod = item.Stats.PercentHPRegenMod,
                        FlatArmorMod = item.Stats.FlatArmorMod,
                        FlatMovementSpeedMod = item.Stats.FlatMovementSpeedMod,
                        FlatHPRegenMod = item.Stats.FlatHPRegenMod,
                        FlatHPPoolMod = item.Stats.FlatHPPoolMod
                    }
                };
                var name = normalizeNames(item.Name);
                var fileName = Path.Combine(leagueItemCacheFolder, name + "." + item.Id + ".cache");
                using (var file = File.Create(fileName))
                {
                    Serializer.Serialize(file, serializedItem);
                }
                itemDict.TryAdd(item.Id, name);
            });

            var indexFileName = Path.Combine(leagueItemCacheFolder, "index.cache");
            itemIndex.Name = "Items";
            itemIndex.Index = Serializer.DeepClone(itemDict);
            using (var file = File.Create(indexFileName))
            {
                Serializer.Serialize(file, itemIndex);
            }
            return Task.CompletedTask;
        }

        #endregion

        #region images

        #endregion

        #region versions

        public Task CreateVersionCache(List<String> versionStringList)
        {
            verIndex.Name = "Versions";
            var counter = 0;

            foreach (var version in versionStringList)
            {
                verDict.TryAdd(counter, version);
                counter++;
            }

            verIndex.Index = Serializer.DeepClone(verDict);
            var fileName = Path.Combine(leagueCacheFolder, "versions.cache");
            using (var file = File.Create(fileName))
            {
                Serializer.Serialize(file, verIndex);
            }
            return Task.CompletedTask;
        }

        #endregion

        # region Common
        public static string normalizeNames(string input, string replacement = "")
        {
            var newString = sWhitespace.Replace(input, replacement);
            return newString.ToLower().Replace("\"", string.Empty).Replace("'", string.Empty);
        }

        public async Task<dynamic> searchCache(string name, LookupDictionary dictionary)
        {
            return await Task.Run(() => _searchCache(name, dictionary));
        }

        public async Task<dynamic> searchCache(int id, LookupDictionary dictionary)
        {
            return await Task.Run(() => _searchCache(id, dictionary));
        }

        private async Task<dynamic> _searchCache<T, T2>(T nameOrId, T2 dictionary) where T2 : LookupDictionary
        {
            if (dictionary.Name == "Champions")
            {
                return await Task.Run(() =>
                {
                    LeagueFileCache.Champion champion;
                    if (typeof(T) == typeof(int)) //the type must be int or String
                    {
                        // the object is an int
                        int key = Convert.ToInt32(nameOrId);
                        var name = "";
                        champDict.TryGetValue(key, out name);
                        if (name != null)
                        {

                            var champName = LeagueFileCacheService.normalizeNames(name);
                            string fullFileName = $"{champName}.{key}.cache";
                            using (var file = File.OpenRead(Path.Combine(leagueChampionCacheFolder, fullFileName)))
                            {
                                champion = Serializer.Deserialize<LeagueFileCache.Champion>(file);
                            }
                            return champion;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        // the object is a string
                        var value = Convert.ToString(nameOrId);
                        var champName = LeagueFileCacheService.normalizeNames(value);
                        var key = champDict.FirstOrDefault(x => x.Value == champName).Key;
                        string fullFileName = $"{champName}.{key}.cache";
                        using (var file = File.OpenRead(Path.Combine(leagueChampionCacheFolder, fullFileName)))
                        {
                            champion = Serializer.Deserialize<LeagueFileCache.Champion>(file);
                        }
                        return champion;
                    }
                });
            }
            else if (dictionary.Name == "Items")
            {
                return await Task.Run(() =>
                {
                    LeagueFileCache.Item item;
                    if (typeof(T) == typeof(int)) //the type must be int or String
                    {
                        // the object is an int
                        int key = Convert.ToInt32(nameOrId);
                        var name = "";
                        itemDict.TryGetValue(key, out name);
                        if (name != null)
                        {

                            var itemName = LeagueFileCacheService.normalizeNames(name);
                            string fullFileName = $"{itemName}.{key}.cache";
                            using (var file = File.OpenRead(Path.Combine(leagueItemCacheFolder, fullFileName)))
                            {
                                item = Serializer.Deserialize<LeagueFileCache.Item>(file);
                            }
                            return item;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        // the object is a string
                        var value = Convert.ToString(nameOrId);
                        var itemName = LeagueFileCacheService.normalizeNames(value);
                        var key = itemDict.FirstOrDefault(x => x.Value == itemName).Key;
                        string fullFileName = $"{itemName}.{key}.cache";
                        using (var file = File.OpenRead(Path.Combine(leagueItemCacheFolder, fullFileName)))
                        {
                            item = Serializer.Deserialize<LeagueFileCache.Item>(file);
                        }
                        return item;
                    }
                });
            }
            else if (dictionary.Name == "LeagueImages")
            {
                return null;
            }
            else if (dictionary.Name == "Versions")
            {
                return await Task.Run<String>(() =>
                {
                    if (typeof(T) == typeof(int)) //the type must be int or String
                    {
                        // the object is an int
                        int key = Convert.ToInt32(nameOrId);
                        var name = "";
                        verDict.TryGetValue(key, out name);
                        if (name != null)
                        {
                            return (string)name;
                        }
                        else
                        {
                            return (string)null;
                        }
                    }
                    else
                    {
                        // the object is a string
                        var value = Convert.ToString(nameOrId);
                        var key = itemDict.FirstOrDefault(x => x.Value == value).Key;
                        return key.ToString();
                    }
                });
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}