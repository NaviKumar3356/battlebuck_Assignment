// PlayerView.cs
// MonoBehaviour that owns ALL visual state for one player capsule.
//
// Architecture decision: PlayerView is the exclusive bridge between gameplay
// events (KillEvent, RespawnEvent) and Unity visual systems (MeshRenderer,
// Transform animations). It never reads or writes PlayerModel data — the
// model is only used at Initialise() time to grab the player name for the label.
//
// Visual systems contained here:
//  • World-space TextMeshPro name label (billboard — always faces camera)
//  • Attack flash  — emissive colour + scale punch coroutine triggered on kill
//  • Death shrink  — scale-to-zero coroutine, then MeshRenderer/label disabled
//  • Respawn pop   — spring scale-from-zero coroutine on respawn

using System.Collections;
using DeathMatch.Player;
using TMPro;
using UnityEngine;

namespace DeathMatch.Player
{
    [RequireComponent(typeof(MeshRenderer))]
    public class PlayerView : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────
        [SerializeField] private PlayerVisualConfig visualConfig;
        [SerializeField] private TextMeshPro nameLabel; // world-space TMP on a child object

        // ── Cached refs ───────────────────────────────────────────
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _mpb; // per-instance — avoids material duplication
        private Transform _labelTransform;
        private Camera _mainCamera;

        // Cached so LateUpdate never allocates
        private Vector3 _baseScale;

        // Active coroutines tracked so we can cancel on overlap
        private Coroutine _rotationCoroutine;
        private Coroutine _attackCoroutine;
        private Coroutine _deathCoroutine;
        private Coroutine _respawnCoroutine;

        // Shader property IDs cached once — avoids repeated string hashing
        private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissionColor");

        // ── Initialisation ────────────────────────────────────────

        /// <summary>Bind this view to a PlayerModel. Called by MatchController after Instantiate.</summary>
        public void Initialise(PlayerModel model, PlayerVisualConfig config = null)
        {
            if (config != null) visualConfig = config;

            _renderer   = GetComponent<MeshRenderer>();
            _mpb        = new MaterialPropertyBlock();
            _baseScale  = transform.localScale;
            _mainCamera = Camera.main;

            if (nameLabel != null)
            {
                _labelTransform        = nameLabel.transform;
                nameLabel.text         = model.Name;
                nameLabel.color        = visualConfig != null ? visualConfig.labelColor : Color.white;
                nameLabel.fontSize     = visualConfig != null ? visualConfig.labelFontSize : 3.5f;
                _labelTransform.localPosition = Vector3.up *
                    (visualConfig != null ? visualConfig.labelHeightOffset : 1.5f);
            }

            // Ensure emissive is zero at start
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissiveColorId, Color.black);
            _renderer.SetPropertyBlock(_mpb);
        }

        // ── Unity lifecycle ───────────────────────────────────────

        private void LateUpdate()
        {
            // Billboard: rotate label to always face the camera without tilting the capsule.
            if (_labelTransform != null && _mainCamera != null)
                _labelTransform.rotation = _mainCamera.transform.rotation;
        }

        // ── Public API (called by MatchController) ────────────────

        /// <summary>Shrink the capsule away then hide it — represents death.</summary>
        public void Hide()
        {
            CancelCoroutine(ref _deathCoroutine);
            CancelCoroutine(ref _respawnCoroutine);
            _deathCoroutine = StartCoroutine(DeathShrink());
        }

        /// <summary>Re-enable the capsule and play a spring pop-in — represents respawn.</summary>
        public void Show()
        {
            CancelCoroutine(ref _deathCoroutine);
            CancelCoroutine(ref _respawnCoroutine);
            _renderer.enabled = true;
            if (nameLabel != null) nameLabel.enabled = true;
            _respawnCoroutine = StartCoroutine(RespawnPop());
        }

        /// <summary>Rotate toward the victim then flash the emissive attack glow.</summary>
        public void PlayAttack(Vector3 targetPosition)
        {
            CancelCoroutine(ref _rotationCoroutine);
            CancelCoroutine(ref _attackCoroutine);
            _rotationCoroutine = StartCoroutine(RotateToward(targetPosition));
            _attackCoroutine   = StartCoroutine(AttackFlash());
        }

        // ── Private coroutines ────────────────────────────────────

        private IEnumerator RotateToward(Vector3 target)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) yield break;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            float speed = visualConfig != null ? visualConfig.rotationSpeed : 280f;

            while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, speed * Time.deltaTime);
                yield return null;
            }
            transform.rotation = targetRot;
        }

        private IEnumerator AttackFlash()
        {
            Color  flashColor    = visualConfig != null ? visualConfig.attackFlashColor    : new Color(1f, 0.35f, 0f);
            float  peakIntensity = visualConfig != null ? visualConfig.attackFlashIntensity : 4f;
            float  riseTime      = visualConfig != null ? visualConfig.attackFlashRiseTime  : 0.08f;
            float  fadeTime      = visualConfig != null ? visualConfig.attackFlashFadeTime  : 0.30f;
            float  scalePeak     = visualConfig != null ? visualConfig.attackScalePeak      : 1.22f;

            // Rise
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / riseTime;
                float ease = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                SetEmissive(flashColor * (ease * peakIntensity));
                transform.localScale = Vector3.LerpUnclamped(_baseScale, _baseScale * scalePeak, ease);
                yield return null;
            }

            // Fade
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeTime;
                float ease = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                SetEmissive(flashColor * ((1f - ease) * peakIntensity));
                transform.localScale = Vector3.Lerp(_baseScale * scalePeak, _baseScale, ease);
                yield return null;
            }

            SetEmissive(Color.black);
            transform.localScale = _baseScale;
        }

        private IEnumerator DeathShrink()
        {
            float shrinkTime = visualConfig != null ? visualConfig.deathShrinkTime : 0.15f;
            float t = 0f;
            Vector3 startScale = transform.localScale;

            while (t < 1f)
            {
                t += Time.deltaTime / shrinkTime;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero,
                    Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
                yield return null;
            }

            transform.localScale = Vector3.zero;
            _renderer.enabled    = false;
            if (nameLabel != null) nameLabel.enabled = false;
        }

        private IEnumerator RespawnPop()
        {
            float popTime = visualConfig != null ? visualConfig.respawnPopTime    : 0.35f;
            float startSc = visualConfig != null ? visualConfig.respawnStartScale : 0.01f;

            transform.localScale = _baseScale * startSc;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / popTime;
                float ease = EaseOutBack(Mathf.Clamp01(t));
                transform.localScale = _baseScale * Mathf.LerpUnclamped(startSc, 1f, ease);
                yield return null;
            }

            transform.localScale = _baseScale;
        }

        // ── Helpers ───────────────────────────────────────────────

        private void SetEmissive(Color color)
        {
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(EmissiveColorId, color);
            _renderer.SetPropertyBlock(_mpb);
        }

        private void CancelCoroutine(ref Coroutine c)
        {
            if (c != null) { StopCoroutine(c); c = null; }
        }

        /// <summary>Cubic ease-out with overshoot — produces a springy pop-in.</summary>
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float tm1 = t - 1f;
            return 1f + c3 * tm1 * tm1 * tm1 + c1 * tm1 * tm1;
        }
    }
}
