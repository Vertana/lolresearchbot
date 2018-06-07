using ProtoBuf;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LolResearchBot.Model
{
    [ProtoContract]
    public class LookupDictionary
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public ConcurrentDictionary<int, string> Index { get; set; }
    }
}