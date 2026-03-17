// MatchTimer.cs
// Pure C# class — no MonoBehaviour.
//
// Architecture decision: the timer tracks elapsed time as plain data.
// It exposes an IEnumerator so MatchController drives the coroutine.
// Firing a TimerTickEvent once per second keeps UI update costs minimal.

using System.Collections;
using DeathMatch.Core;
using UnityEngine;

namespace DeathMatch.Match
{
    public class MatchTimer
    {
        public float RemainingSeconds { get; private set; }
        public bool IsFinished => RemainingSeconds <= 0f;

        private bool _running;

        public MatchTimer(float duration)
        {
            RemainingSeconds = duration;
        }

        /// <summary>Immediately halts the timer loop.</summary>
        public void Stop() => _running = false;

        /// <summary>
        /// IEnumerator driven by MatchController.
        /// Fires TimerTickEvent once per second and signals end via MatchEndedEvent.
        /// </summary>
        public IEnumerator TimerLoop()
        {
            _running = true;

            // Broadcast initial tick so UI shows correct value on match start.
            EventBus.Raise(new TimerTickEvent(RemainingSeconds));

            var oneSecond = new WaitForSeconds(1f);

            while (_running && RemainingSeconds > 0f)
            {
                yield return oneSecond;

                if (!_running) yield break;

                RemainingSeconds = Mathf.Max(0f, RemainingSeconds - 1f);
                EventBus.Raise(new TimerTickEvent(RemainingSeconds));
            }
        }
    }
}