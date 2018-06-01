using System;
using RiotSharp.MatchEndpoint;

namespace LolResearchBot.Model.Extensions
{
    public class ParticipantExtend
    {
        public ParticipantExtend(Participant participant, ParticipantIdentity participantIdentity)
        {
            Participant = participant ?? throw new ArgumentNullException(nameof(participant));
            ParticipantIdentity = participantIdentity ?? throw new ArgumentNullException(nameof(participantIdentity));
        }

        public Participant Participant { get; }

        public ParticipantIdentity ParticipantIdentity { get; }

        public string SummonerName => ParticipantIdentity.Player.SummonerName;
    }
}