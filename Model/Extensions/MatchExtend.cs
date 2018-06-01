using System;
using System.Linq;
using RiotSharp.MatchEndpoint;

namespace LolResearchBot.Model.Extensions
{
    public class MatchExtend
    {
        private TeamStatsExtend[] _teamStats;

        public MatchExtend(Match match)
        {
            Match = match ?? throw new ArgumentNullException(nameof(match));
        }

        public Match Match { get; }

        public TeamStatsExtend[] Teams
        {
            get
            {
                if (_teamStats != null) return _teamStats;


                _teamStats = GetTeamStatsWrapper();

                return _teamStats;
            }
        }

        private TeamStatsExtend[] GetTeamStatsWrapper()
        {
            var teamStatsWrapper = new TeamStatsExtend[Match.Teams.Count];

            for (var index = 0; index < Match.Teams.Count; index++)
            {
                var teamStats = Match.Teams[index];

                var participantWrapper = Match.Participants
                    .Where(p => p.TeamId == teamStats.TeamId)
                    .OrderByDescending(p => p.Stats.Kills + p.Stats.Assists * .5 - p.Stats.Deaths)
                    .Select(p => new ParticipantExtend(p,
                        Match.ParticipantIdentities.SingleOrDefault(x => x.ParticipantId == p.ParticipantId)))
                    .ToArray();

                teamStatsWrapper[index] = new TeamStatsExtend(teamStats, participantWrapper);
            }

            return teamStatsWrapper;
        }
    }
}