// MatchRules.cs
// ScriptableObject that stores all configurable match parameters.
//
// Architecture decision: using a ScriptableObject means designers can tweak
// rules without touching code. Multiple rule presets (quick match, long match)
// can coexist as separate assets and be swapped in the Inspector.

using UnityEngine;

namespace DeathMatch.Match
{
    [CreateAssetMenu(menuName = "DeathMatch/Match Rules", fileName = "MatchRules")]
    public class MatchRules : ScriptableObject
    {
        [Header("Players")]
        [Tooltip("Total players spawned at match start.")]
        public int playerCount = 10;

        [Header("Match Duration")]
        [Tooltip("Match length in seconds. Match ends when this elapses OR score limit is reached.")]
        public float matchDuration = 120f;

        [Header("Respawn")]
        [Tooltip("Seconds a player waits before reappearing after death.")]
        public float respawnTime = 3f;

        [Header("Kill Simulation")]
        [Tooltip("Minimum seconds between kill events.")]
        public float killIntervalMin = 1f;

        [Tooltip("Maximum seconds between kill events.")]
        public float killIntervalMax = 2f;

        [Header("Scoring")]
        [Tooltip("Points awarded per kill.")]
        public int pointsPerKill = 1;

        [Tooltip("First player to reach this score wins (0 = disabled).")]
        public int scoreLimit = 15;
    }
}