// ScoreSystem.cs
// Pure C# class. Owns kill-to-score logic.
//
// Architecture decision: scoring rules (e.g. bonus kills, streaks) are
// isolated here so they can evolve independently. Fires ScoreChangedEvent
// so the leaderboard and UI react without polling.

using DeathMatch.Core;
using DeathMatch.Match;
using DeathMatch.Player;

namespace DeathMatch.Score
{
    public class ScoreSystem
    {
        private readonly MatchRules _rules;

        public ScoreSystem(MatchRules rules)
        {
            _rules = rules;
        }

        /// <summary>Award points to the killer and broadcast a ScoreChangedEvent.</summary>
        public void RegisterKill(PlayerModel killer)
        {
            killer.AddScore(_rules.pointsPerKill);
            EventBus.Raise(new ScoreChangedEvent(killer.Id, killer.Score));
        }
    }
}