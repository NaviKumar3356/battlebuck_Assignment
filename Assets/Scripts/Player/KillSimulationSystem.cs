// KillSimulationSystem.cs
// Pure C# class. Does NOT extend MonoBehaviour.
//
// Architecture decision: the kill loop logic lives here as plain C#.
// The coroutine is driven by MatchController (a MonoBehaviour) via
// the IEnumerator it exposes — keeping this class independently testable
// while still benefiting from Unity's coroutine scheduler.

using System;
using System.Collections;
using System.Collections.Generic;
using DeathMatch.Core;
using DeathMatch.Match;
using UnityEngine;

namespace DeathMatch.Player
{
    public class KillSimulationSystem
    {
        private readonly List<PlayerModel> _players;
        private readonly MatchRules _rules;
        private readonly System.Random _random;

        private bool _running;

        public KillSimulationSystem(List<PlayerModel> players, MatchRules rules)
        {
            _players = players;
            _rules = rules;
            // Use a seeded random so behaviour is reproducible for debugging.
            _random = new System.Random(Environment.TickCount);
        }

        /// <summary>Signals the kill loop to stop processing.</summary>
        public void Stop() => _running = false;

        /// <summary>
        /// Returns an IEnumerator suitable for StartCoroutine.
        /// Randomly pairs a killer and victim every 1-2 seconds.
        /// </summary>
        public IEnumerator KillLoop()
        {
            _running = true;

            while (_running)
            {
                float wait = (float)(_rules.killIntervalMin
                    + _random.NextDouble() * (_rules.killIntervalMax - _rules.killIntervalMin));

                yield return new WaitForSeconds(wait);

                if (!_running) yield break;

                PlayerModel killer = GetRandomAlivePlayer(excludeId: -1);
                if (killer == null) continue;

                PlayerModel victim = GetRandomAlivePlayer(excludeId: killer.Id);
                if (victim == null) continue;

                // Fire the event — all subscribers (ScoreSystem, RespawnSystem, Views) react.
                EventBus.Raise(new KillEvent(killer.Id, victim.Id));
            }
        }

        // ── Private helpers ───────────────────────────────────────

        /// <summary>Returns a random alive player, optionally excluding one ID.</summary>
        private PlayerModel GetRandomAlivePlayer(int excludeId)
        {
            // Build a temporary list without allocation by scanning with a random offset.
            int count = _players.Count;
            int startIndex = _random.Next(0, count);

            for (int i = 0; i < count; i++)
            {
                int idx = (startIndex + i) % count;
                PlayerModel candidate = _players[idx];
                if (candidate.IsAlive && candidate.Id != excludeId)
                    return candidate;
            }

            return null; // No valid candidate found.
        }
    }
}