// GameEvents.cs
// Defines all event payload structs used across the event bus.
// Using readonly structs avoids heap allocations when events are fired frequently.

namespace DeathMatch.Core
{
    /// <summary>Fired when a player successfully kills another player.</summary>
    public readonly struct KillEvent
    {
        public readonly int KillerPlayerId;
        public readonly int VictimPlayerId;

        public KillEvent(int killerId, int victimId)
        {
            KillerPlayerId = killerId;
            VictimPlayerId = victimId;
        }
    }

    /// <summary>Fired when a player's score changes.</summary>
    public readonly struct ScoreChangedEvent
    {
        public readonly int PlayerId;
        public readonly int NewScore;

        public ScoreChangedEvent(int playerId, int newScore)
        {
            PlayerId = playerId;
            NewScore = newScore;
        }
    }

    /// <summary>Fired every second with the remaining match time.</summary>
    public readonly struct TimerTickEvent
    {
        public readonly float RemainingSeconds;

        public TimerTickEvent(float remaining) => RemainingSeconds = remaining;
    }

    /// <summary>Fired when the match ends. Contains the winning player id (-1 if draw).</summary>
    public readonly struct MatchEndedEvent
    {
        public readonly int WinnerPlayerId;
        public readonly string WinnerName;

        public MatchEndedEvent(int winnerId, string winnerName)
        {
            WinnerPlayerId = winnerId;
            WinnerName = winnerName;
        }
    }

    /// <summary>Fired when a player is scheduled to respawn.</summary>
    public readonly struct RespawnEvent
    {
        public readonly int PlayerId;

        public RespawnEvent(int playerId) => PlayerId = playerId;
    }

    /// <summary>Fired once when the match is fully initialised and starts.</summary>
    public readonly struct MatchStartedEvent { }
}
