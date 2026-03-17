// PlayerRespawnSystem.cs
// Pure C# class. Tracks pending respawns and fires RespawnEvent when each timer elapses.
//
// Architecture decision: respawn timers are tracked here as plain C# data.
// The coroutines are driven by MatchController via GetRespawnCoroutine().
// This avoids coupling this class to MonoBehaviour or CoroutineRunner.

using System.Collections;
using System.Collections.Generic;
using DeathMatch.Core;
using DeathMatch.Match;
using UnityEngine;

namespace DeathMatch.Player
{
    public class PlayerRespawnSystem
    {
        private readonly MatchRules _rules;
        // Tracks how many active respawn coroutines are pending per player id
        // so we don't stack multiple respawns for the same player.
        private readonly HashSet<int> _pendingRespawns = new HashSet<int>();

        public PlayerRespawnSystem(MatchRules rules)
        {
            _rules = rules;
        }

        /// <summary>
        /// Returns true if a respawn was scheduled; false if one is already pending for this player.
        /// The caller is responsible for starting the returned coroutine.
        /// </summary>
        public bool TryScheduleRespawn(PlayerModel player, out IEnumerator coroutine)
        {
            if (_pendingRespawns.Contains(player.Id))
            {
                coroutine = null;
                return false;
            }

            _pendingRespawns.Add(player.Id);
            coroutine = RespawnRoutine(player);
            return true;
        }

        // ── Private ───────────────────────────────────────────────

        private IEnumerator RespawnRoutine(PlayerModel player)
        {
            yield return new WaitForSeconds(_rules.respawnTime);

            player.Respawn();
            _pendingRespawns.Remove(player.Id);

            // Notify the view layer so the capsule re-appears.
            EventBus.Raise(new RespawnEvent(player.Id));
        }
    }
}