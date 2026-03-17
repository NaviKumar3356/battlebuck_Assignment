// GameBootstrap.cs
// Clears the EventBus before any other MonoBehaviour Awake runs.
// [DefaultExecutionOrder(-100)] guarantees this executes first in the frame.

using DeathMatch.Core;
using UnityEngine;

namespace DeathMatch.Core
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // Clear stale subscriptions from any previous play session or scene reload.
            EventBus.Clear();
        }
    }
}