// CoroutineRunner.cs
// Persistent singleton MonoBehaviour used to run coroutines from plain-C# classes
// when they cannot call StartCoroutine directly.
//
// Architecture decision: kept as a fallback utility. In this project,
// MatchController owns all coroutines so CoroutineRunner is provided
// for extensibility (e.g. future powerup systems, cinematic helpers).

using UnityEngine;

namespace DeathMatch.Utils
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("[CoroutineRunner]");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}