// MatchUIController.cs
// MonoBehaviour that owns all UI sub-components and connects them to MatchController.
//
// Architecture decision: subscribes to MatchStartedEvent in Awake (not OnEnable)
// because the event is raised in MatchController.Start — which runs after all
// Awakes are done but before the first Update. Subscribing in Awake ensures
// the handler is registered before Start on any component can fire it.

using DeathMatch.Core;
using DeathMatch.Match;
using UnityEngine;

namespace DeathMatch.UI
{
    [DefaultExecutionOrder(5)] // After GameBootstrap(-100), before MatchController(10)
    public class MatchUIController : MonoBehaviour
    {
        // ── Inspector fields ──────────────────────────────────────
        [SerializeField] private MatchController matchController;
        [SerializeField] private UILeaderboard leaderboard;
        [SerializeField] private UITimer timer;
        [SerializeField] private UIWinnerScreen winnerScreen;

        // ── Unity lifecycle ───────────────────────────────────────

        private void Awake()
        {
            // Subscribe here so the handler is live when MatchController.Start fires the event.
            EventBus.Subscribe<MatchStartedEvent>(OnMatchStarted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<MatchStartedEvent>(OnMatchStarted);
        }

        // ── Event handlers ────────────────────────────────────────

        private void OnMatchStarted(MatchStartedEvent e)
        {
            if (matchController == null)
            {
                Debug.LogError("[MatchUIController] MatchController reference is missing.");
                return;
            }

            // Hand the leaderboard the sorted snapshot. The same list reference is
            // updated in-place by LeaderboardSystem, so UILeaderboard always reads
            // the latest order on every RebuildText call.
            leaderboard?.Initialise(matchController.GetLeaderboard());
        }
    }
}