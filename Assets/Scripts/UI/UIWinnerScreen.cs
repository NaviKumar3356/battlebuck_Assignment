// UIWinnerScreen.cs
// MonoBehaviour that displays the winner panel when the match ends.
//
// Architecture decision: the panel is hidden via CanvasGroup.alpha = 0 and
// interactable/blocksRaycasts = false rather than SetActive(false). This keeps
// the component alive so its OnEnable subscription to MatchEndedEvent is
// established at scene start and not missed.

using DeathMatch.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeathMatch.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIWinnerScreen : MonoBehaviour
    {
        // ── Inspector fields ──────────────────────────────────────
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Button restartButton;

        private CanvasGroup _canvasGroup;

        // ── Unity lifecycle ───────────────────────────────────────

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            // Hide visually without deactivating — keeps OnEnable/OnDisable working.
            SetVisible(false);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartPressed);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<MatchEndedEvent>(OnMatchEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MatchEndedEvent>(OnMatchEnded);
        }

        // ── Event handlers ────────────────────────────────────────

        private void OnMatchEnded(MatchEndedEvent e)
        {
            if (winnerText != null)
                winnerText.text = e.WinnerName;

            if (subtitleText != null)
                subtitleText.text = e.WinnerPlayerId >= 0 ? "WINS THE MATCH!" : "DRAW!";

            SetVisible(true);
        }

        // ── Button handlers ───────────────────────────────────────

        private void OnRestartPressed()
        {
            EventBus.Clear();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        // ── Helpers ───────────────────────────────────────────────

        private void SetVisible(bool visible)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha          = visible ? 1f : 0f;
            _canvasGroup.interactable   = visible;
            _canvasGroup.blocksRaycasts = visible;
        }
    }
}