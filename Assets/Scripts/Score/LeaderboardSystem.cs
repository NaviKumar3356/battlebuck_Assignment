// LeaderboardSystem.cs
// Pure C# class. Maintains a sorted leaderboard snapshot.
//
// Architecture decision: the leaderboard is only re-sorted when Invalidate()
// is called (i.e. after a kill), never on every frame. The sorted list is a
// pre-allocated snapshot to avoid per-call allocations.

using System.Collections.Generic;
using DeathMatch.Player;

namespace DeathMatch.Score
{
    public class LeaderboardSystem
    {
        private readonly List<PlayerModel> _source;
        // Pre-allocated snapshot list — reused on every sort to avoid GC pressure.
        private readonly List<PlayerModel> _sortedSnapshot;
        private bool _dirty = true;

        public LeaderboardSystem(List<PlayerModel> players)
        {
            _source = players;
            _sortedSnapshot = new List<PlayerModel>(players.Count);
        }

        /// <summary>Mark the leaderboard as stale so the next call re-sorts it.</summary>
        public void Invalidate() => _dirty = true;

        /// <summary>Returns the sorted snapshot. Only re-sorts when dirty.</summary>
        public List<PlayerModel> GetSortedSnapshot()
        {
            if (_dirty)
            {
                _sortedSnapshot.Clear();
                _sortedSnapshot.AddRange(_source);
                // Insertion sort is efficient for small, nearly-sorted lists (≤ 10–15 players).
                InsertionSortDescending(_sortedSnapshot);
                _dirty = false;
            }
            return _sortedSnapshot;
        }

        /// <summary>Returns the player with the highest current score, or null.</summary>
        public PlayerModel GetLeader()
        {
            List<PlayerModel> sorted = GetSortedSnapshot();
            return sorted.Count > 0 ? sorted[0] : null;
        }

        // ── Private ───────────────────────────────────────────────

        private static void InsertionSortDescending(List<PlayerModel> list)
        {
            int n = list.Count;
            for (int i = 1; i < n; i++)
            {
                PlayerModel key = list[i];
                int j = i - 1;
                while (j >= 0 && list[j].Score < key.Score)
                {
                    list[j + 1] = list[j];
                    j--;
                }
                list[j + 1] = key;
            }
        }
    }
}