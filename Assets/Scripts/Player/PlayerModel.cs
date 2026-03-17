// PlayerModel.cs
// Pure C# data model — no MonoBehaviour, no Unity dependencies.
//
// Architecture decision: separating data (PlayerModel) from presentation
// (PlayerView) follows the Model-View pattern. Systems that only need to
// read/write player data never have to touch a GameObject, making logic
// portable and independently testable.

namespace DeathMatch.Player
{
    public class PlayerModel
    {
        // ── Identity ──────────────────────────────────────────────
        public int Id { get; }
        public string Name { get; }

        // ── Runtime state ─────────────────────────────────────────
        public int Score { get; private set; }
        public bool IsAlive { get; private set; }
        public int KillCount { get; private set; }
        public int DeathCount { get; private set; }

        public PlayerModel(int id, string name)
        {
            Id = id;
            Name = name;
            Score = 0;
            IsAlive = true;
        }

        /// <summary>Increment kill counter and score by the given amount.</summary>
        public void AddScore(int amount)
        {
            Score += amount;
            KillCount++;
        }

        /// <summary>Mark the player as dead and increment death counter.</summary>
        public void RegisterDeath()
        {
            IsAlive = false;
            DeathCount++;
        }

        /// <summary>Bring the player back to life.</summary>
        public void Respawn() => IsAlive = true;
    }
}