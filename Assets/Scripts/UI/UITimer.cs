// UITimer.cs
// MonoBehaviour that displays remaining match time.
//
// Mobile optimisation: timer text is only rebuilt when the second changes
// (driven by event, not Update). StringBuilder avoids string allocations.

using System.Text;
using DeathMatch.Core;
using TMPro;
using UnityEngine;

namespace DeathMatch.UI
{
    public class UITimer : MonoBehaviour
    {
        // ── Inspector fields ──────────────────────────────────────
        [SerializeField] private TextMeshProUGUI timerText;

        private readonly StringBuilder _sb = new StringBuilder(16);

        // ── Unity lifecycle ───────────────────────────────────────

        private void Awake()
        {
            EventBus.Subscribe<TimerTickEvent>(OnTimerTick);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TimerTickEvent>(OnTimerTick);
        }

        // ── Event handlers ────────────────────────────────────────

        private void OnTimerTick(TimerTickEvent e)
        {
            int totalSeconds = Mathf.CeilToInt(e.RemainingSeconds);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            _sb.Clear();
            if (minutes > 0)
            {
                _sb.Append(minutes);
                _sb.Append(':');
                if (seconds < 10) _sb.Append('0');
            }
            _sb.Append(seconds);

            timerText.SetText(_sb);
        }
    }
}