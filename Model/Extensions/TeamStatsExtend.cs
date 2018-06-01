using System;
using System.Linq;
using RiotSharp.MatchEndpoint;

namespace LolResearchBot.Model.Extensions
{
    public class TeamStatsExtend
    {
        public TeamStatsExtend(TeamStats teamStats, params ParticipantExtend[] participants)
        {
            Stats = teamStats ?? throw new ArgumentNullException(nameof(teamStats));
            Participants = participants;
        }


        //List already sort from best to worsts so grab the last Participant
        public ParticipantExtend WorstParticipant => Participants.Last();

        public TeamStats Stats { get; }

        public ParticipantExtend[] Participants { get; }
    }
}