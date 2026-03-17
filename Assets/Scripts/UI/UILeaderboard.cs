// UILeaderboard.cs
// MonoBehaviour that renders the live leaderboard.
//
// Architecture decision: the UI layer only reads data, never writes it.
// It reacts to ScoreChangedEvent fired by the event bus — so it is
// never updated more often than kills actually happen (mobile-friendly).
//
// Mobile optimisation: StringBuilder is reused across rebuilds to avoid
// per-update string allocations. No LINQ. No string concatenation in hot path.

using System.Collections.Generic;
using System.Text;
using DeathMatch.Core;
using DeathMatch.Player;
using TMPro;
using UnityEngine;

namespace DeathMatch.UI
{
    public class UILeaderboard : MonoBehaviour
    {
        // ── Inspector fields ──────────────────────────────────────
        [SerializeField] private TextMeshProUGUI leaderboardText;

        // ── Runtime state ─────────────────────────────────────────
        private List<PlayerModel> _sortedPlayers;
        // Pre-allocated StringBuilder avoids GC allocations on every redraw.
        private readonly StringBuilder _sb = new StringBuilder(512);

        // ── Public API ────────────────────────────────────────────

        /// <summary>Called by MatchUIController after leaderboard data is ready.</summary>
        public void Initialise(List<PlayerModel> sortedPlayers)
        {
            _sortedPlayers = sortedPlayers;
            RebuildText();
        }

        // ── Unity lifecycle ───────────────────────────────────────

        private void Awake()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        // ── Event handlers ────────────────────────────────────────

        private void OnScoreChanged(ScoreChangedEvent e)
        {
            // The sorted list is already updated by LeaderboardSystem before this event fires.
            RebuildText();
        }

        // ── Private ───────────────────────────────────────────────

        private void RebuildText()
        {
            if (_sortedPlayers == null || leaderboardText == null) return;

            _sb.Clear();
            _sb.AppendLine("<b>LEADERBOARD</b>");
            _sb.AppendLine("─────────────────");

            int count = _sortedPlayers.Count;
            for (int i = 0; i < count; i++)
            {
                PlayerModel p = _sortedPlayers[i];
                _sb.Append(i + 1);
                _sb.Append(". ");
                _sb.Append(p.Name);
                _sb.Append("  —  ");
                _sb.Append(p.Score);
                _sb.AppendLine(" pts");
            }

            leaderboardText.SetText(_sb);
        }
    }
}