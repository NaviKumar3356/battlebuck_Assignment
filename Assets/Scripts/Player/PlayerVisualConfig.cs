// PlayerVisualConfig.cs
// ScriptableObject that holds every visual tuning parameter for PlayerView.
//
// Architecture decision: all magic numbers for animations, colours, and timings
// live here instead of being hard-coded in PlayerView. Designers can iterate
// without touching code, and multiple visual presets can coexist as separate assets.

using UnityEngine;

namespace DeathMatch.Player
{
    [CreateAssetMenu(menuName = "DeathMatch/Player Visual Config", fileName = "PlayerVisualConfig")]
    public class PlayerVisualConfig : ScriptableObject
    {
        [Header("Rotation")]
        [Tooltip("Degrees per second the killer rotates toward the victim.")]
        public float rotationSpeed = 280f;

        [Header("Attack Flash")]
        [Tooltip("Emissive colour punched on the killer capsule when a kill is registered.")]
        public Color attackFlashColor = new Color(1f, 0.35f, 0f, 1f);   // hot orange

        [Tooltip("Peak emissive intensity multiplier during the flash.")]
        public float attackFlashIntensity = 4f;

        [Tooltip("Time in seconds for the flash to reach peak brightness.")]
        public float attackFlashRiseTime = 0.08f;

        [Tooltip("Time in seconds for the flash to fade back to zero.")]
        public float attackFlashFadeTime = 0.30f;

        [Header("Attack Scale Punch")]
        [Tooltip("Scale multiplier applied at the peak of the attack punch.")]
        public float attackScalePeak = 1.22f;

        [Tooltip("Time in seconds to reach peak scale.")]
        public float attackScaleRiseTime = 0.06f;

        [Tooltip("Time in seconds to return to normal scale.")]
        public float attackScaleFadeTime = 0.18f;

        [Header("Respawn Pop-In")]
        [Tooltip("Scale the capsule starts at when it respawns.")]
        public float respawnStartScale = 0.01f;

        [Tooltip("Time in seconds for the pop-in to full size.")]
        public float respawnPopTime = 0.35f;

        [Header("Death Shrink")]
        [Tooltip("Time in seconds for the capsule to shrink to zero on death.")]
        public float deathShrinkTime = 0.15f;

        [Header("Label")]
        [Tooltip("Colour of the player name label above the capsule.")]
        public Color labelColor = new Color(1f, 1f, 1f, 1f);

        [Tooltip("Font size of the world-space name label.")]
        public float labelFontSize = 3.5f;

        [Tooltip("Height offset of the label above the capsule pivot.")]
        public float labelHeightOffset = 1.5f;
    }
}
