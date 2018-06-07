using ProtoBuf;
using System.Collections.Generic;

namespace LolResearchBot.Model
{
    public class LeagueFileCache
    {
        [ProtoContract]
        public class Champion
        {
            [ProtoMember(1)]
            public int Id { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public string Lore { get; set; }
            [ProtoMember(4)]
            public string Partype { get; set; }
            [ProtoMember(5)]
            public Image Image { get; set; }
            [ProtoMember(6)]
            public ChampStats Stats { get; set; }
            [ProtoMember(7)]
            public Passive Passive { get; set; }
            [ProtoMember(8)]
            public Info Info { get; set; }
        }

        [ProtoContract]
        public class Item
        {
            [ProtoMember(1)]
            public int Id { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public string Description { get; set; }
            [ProtoMember(4)]
            public string sanitizedDescription { get; set; }
            [ProtoMember(5)]
            public int BasePrice { get; set; }
            [ProtoMember(6)]
            public int SellingPrice { get; set; }
            [ProtoMember(7)]
            public int TotalPrice { get; set; }
            [ProtoMember(8)]
            public bool Purchasable { get; set; }
            [ProtoMember(9)]
            public List<string> BuiltFrom { get; set; } // Contains a string list of item ID's it is built from
            [ProtoMember(10)]
            public ItemStats Stats { get; set; }
        }
        [ProtoContract]
        public class ChampStats
        {
            [ProtoMember(1)]
            public double armorPerLevel { get; set; }
            [ProtoMember(2)]
            public double hpPerLevel { get; set; }
            [ProtoMember(3)]
            public double mpPerLevel { get; set; }
            [ProtoMember(4)]
            public double armor { get; set; }
            [ProtoMember(5)]
            public double hp { get; set; }
            [ProtoMember(6)]
            public double critPerLevel { get; set; }
        }
        [ProtoContract]
        public class Passive
        {
            [ProtoMember(1)]
            public string sanitizedDescription { get; set; }
            [ProtoMember(2)]
            public string Name { get; set; }
            [ProtoMember(3)]
            public string Description { get; set; }
        }

        [ProtoContract]
        public class Info
        {
            [ProtoMember(1)]
            public int Attack { get; set; }
            [ProtoMember(2)]
            public int Defense { get; set; }
            [ProtoMember(3)]
            public int Difficulty { get; set; }
            [ProtoMember(4)]
            public int Magic { get; set; }
        }

        [ProtoContract]
        public class Image
        {
            [ProtoMember(1)]
            public string Full { get; set; }
            [ProtoMember(2)]
            public string Group { get; set; }
            [ProtoMember(3)]
            public string Sprite { get; set; }
            [ProtoMember(4)]
            public int h { get; set; }
            [ProtoMember(5)]
            public int w { get; set; }
            [ProtoMember(6)]
            public int y { get; set; }
            [ProtoMember(7)]
            public int x { get; set; }
        }

        [ProtoContract]
        public class ItemStats
        {
            [ProtoMember(1)]
            public double PercentMovementSpeedMod { get; set; }
            [ProtoMember(2)]
            public double PercentLifeStealMod { get; set; }
            [ProtoMember(3)]
            public double PercentAttackSpeedMod { get; set; }
            [ProtoMember(4)]
            public double PercentArmorMod { get; set; }
            [ProtoMember(5)]
            public double PercentCooldownMod { get; set; }
            [ProtoMember(6)]
            public double PercentCritChanceMod { get; set; }
            [ProtoMember(7)]
            public double PercentCritDamageMod { get; set; }
            [ProtoMember(8)]
            public double PercentHPRegenMod { get; set; }
            [ProtoMember(9)]
            public double FlatArmorMod { get; set; }
            [ProtoMember(10)]
            public double FlatMovementSpeedMod { get; set; }
            [ProtoMember(11)]
            public double FlatHPRegenMod { get; set; }
            [ProtoMember(12)]
            public double FlatHPPoolMod { get; set; }
        }
    }
}