// MatchController.cs
// The sole MonoBehaviour orchestrator for the match.
//
// Architecture decision: MatchController is the only MonoBehaviour that
// "owns" and wires together all plain-C# systems. It is deliberately thin —
// it does not contain game logic. It delegates to specialised classes and
// starts/stops coroutines on their behalf. This prevents a God Object while
// still having a single entry point in the scene.

using System.Collections;
using System.Collections.Generic;
using DeathMatch.Core;
using DeathMatch.Match;
using DeathMatch.Player;
using DeathMatch.Score;
using UnityEngine;

namespace DeathMatch.Match
{
    // [DefaultExecutionOrder(10)] ensures Awake runs after GameBootstrap(-100)
    // and after all UI MonoBehaviours have called OnEnable (which happens
    // between Awake and Start). MatchStartedEvent is raised in Start so every
    // subscriber's OnEnable has already fired.
    [DefaultExecutionOrder(10)]
    public class MatchController : MonoBehaviour
    {
        // ── Inspector fields ──────────────────────────────────────
        [Header("Configuration")]
        [SerializeField] private MatchRules rules;

        [Header("Arena")]
        [Tooltip("Prefab with a PlayerView component. A capsule with material is fine.")]
        [SerializeField] private GameObject playerPrefab;

        [Tooltip("Radius of the circle on which players are spawned.")]
        [SerializeField] private float spawnRadius = 8f;

        [Header("Visuals")]
        [Tooltip("Visual tuning config shared across all PlayerView instances.")]
        [SerializeField] private DeathMatch.Player.PlayerVisualConfig visualConfig;

        // ── Systems (plain C# classes) ────────────────────────────
        private List<PlayerModel> _playerModels;
        private Dictionary<int, PlayerView> _playerViews;

        private ScoreSystem _scoreSystem;
        private PlayerRespawnSystem _respawnSystem;
        private LeaderboardSystem _leaderboardSystem;
        private KillSimulationSystem _killSimulator;
        private MatchTimer _matchTimer;

        private bool _matchOver;

        // ── Unity lifecycle ───────────────────────────────────────

        private void Awake()
        {
            if (rules == null)
            {
                Debug.LogError("[MatchController] MatchRules ScriptableObject is not assigned!");
                return;
            }

            InitialiseSystems();
        }

        // Start is called after all Awakes AND after all OnEnables in the scene.
        // Raising MatchStartedEvent here guarantees every subscriber is already listening.
        private void Start()
        {
            if (rules == null) return;
            EventBus.Raise(new MatchStartedEvent());
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<KillEvent>(OnKillEvent);
            EventBus.Unsubscribe<RespawnEvent>(OnRespawnEvent);
        }

        // ── Initialisation ────────────────────────────────────────

        private void InitialiseSystems()
        {
            // 1. Create data models.
            _playerModels = PlayerSpawner.CreateModels(rules.playerCount);

            // 2. Spawn GameObjects and pair each with its model.
            _playerViews = new Dictionary<int, PlayerView>(_playerModels.Count);
            SpawnPlayerObjects();

            // 3. Create pure-C# systems.
            _scoreSystem = new ScoreSystem(rules);
            _respawnSystem = new PlayerRespawnSystem(rules);
            _leaderboardSystem = new LeaderboardSystem(_playerModels);
            _killSimulator = new KillSimulationSystem(_playerModels, rules);
            _matchTimer = new MatchTimer(rules.matchDuration);

            // 4. Subscribe to kill and respawn events.
            EventBus.Subscribe<KillEvent>(OnKillEvent);
            EventBus.Subscribe<RespawnEvent>(OnRespawnEvent);

            // 5. Start coroutine-driven systems.
            StartCoroutine(_matchTimer.TimerLoop());
            StartCoroutine(_killSimulator.KillLoop());
            StartCoroutine(WatchTimerEnd());

            // MatchStartedEvent is raised in Start() — not here — so all
            // OnEnable subscriptions across the scene are guaranteed to be live.
        }

        // ── Spawning ──────────────────────────────────────────────

        private void SpawnPlayerObjects()
        {
            int count = _playerModels.Count;
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                // Offset angle by 90° so first player spawns at the top of the circle (positive Z)
                float angle = (i * angleStep + 90f) * Mathf.Deg2Rad;

                Vector3 position = new Vector3(
                    Mathf.Cos(angle) * spawnRadius,
                    1f,                              // y=1 so capsule (height 2) sits on the floor
                    Mathf.Sin(angle) * spawnRadius);

                // Face inward toward the arena centre
                Quaternion rotation = Quaternion.LookRotation(-position.normalized, Vector3.up);

                GameObject go = Instantiate(playerPrefab, position, rotation);
                go.name = $"Player_{_playerModels[i].Name}";

                PlayerView view = go.GetComponent<PlayerView>();
                if (view == null)
                    view = go.AddComponent<PlayerView>();

                view.Initialise(_playerModels[i], visualConfig);
                _playerViews[_playerModels[i].Id] = view;
            }
        }

        // ── Event handlers ────────────────────────────────────────

        private void OnKillEvent(KillEvent e)
        {
            if (_matchOver) return;

            // Retrieve models.
            PlayerModel killer = FindModel(e.KillerPlayerId);
            PlayerModel victim = FindModel(e.VictimPlayerId);

            if (killer == null || victim == null) return;

            // 1. Update score data.
            _scoreSystem.RegisterKill(killer);

            // 2. Mark victim dead.
            victim.RegisterDeath();

            // 3. Visual feedback — killer plays attack animation, victim disappears.
            if (_playerViews.TryGetValue(killer.Id, out PlayerView killerView) &&
                _playerViews.TryGetValue(victim.Id, out PlayerView victimView))
            {
                killerView.PlayAttack(victimView.transform.position);
                victimView.Hide();
            }

            // 4. Schedule respawn.
            if (_respawnSystem.TryScheduleRespawn(victim, out IEnumerator respawnCoroutine))
                StartCoroutine(respawnCoroutine);

            // 5. Notify leaderboard to re-sort.
            _leaderboardSystem.Invalidate();

            // 6. Check score-limit win condition.
            if (rules.scoreLimit > 0 && killer.Score >= rules.scoreLimit)
            {
                EndMatch(killer);
            }
        }

        private void OnRespawnEvent(RespawnEvent e)
        {
            if (_playerViews.TryGetValue(e.PlayerId, out PlayerView view))
                view.Show();
        }

        // ── Win conditions ────────────────────────────────────────
        /// <summary>Watches for timer expiry and ends the match when it hits zero.</summary>
        private IEnumerator WatchTimerEnd()
        {
            yield return new WaitUntil(() => _matchTimer.IsFinished || _matchOver);

            if (!_matchOver)
            {
                PlayerModel leader = _leaderboardSystem.GetLeader();
                EndMatch(leader);
            }
        }

        private void EndMatch(PlayerModel winner)
        {
            if (_matchOver) return;
            _matchOver = true;

            _killSimulator.Stop();
            _matchTimer.Stop();

            string winnerName = winner != null ? winner.Name : "Nobody";
            int winnerId = winner != null ? winner.Id : -1;

            EventBus.Raise(new MatchEndedEvent(winnerId, winnerName));
        }

        // ── Helpers ───────────────────────────────────────────────

        private PlayerModel FindModel(int id)
        {
            // Linear scan is fine for ≤ 10–15 players and avoids a second Dictionary.
            for (int i = 0; i < _playerModels.Count; i++)
            {
                if (_playerModels[i].Id == id)
                    return _playerModels[i];
            }
            return null;
        }

        // ── Public API (consumed by UI) ───────────────────────────

        /// <summary>Returns the sorted leaderboard snapshot for UI rendering.</summary>
        public List<PlayerModel> GetLeaderboard() => _leaderboardSystem.GetSortedSnapshot();
    }
}