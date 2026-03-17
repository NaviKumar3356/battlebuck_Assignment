// SkyboxApplier.cs
// Tiny visual utility MonoBehaviour that applies a skybox material to
// RenderSettings at Awake. Completely visual — no gameplay logic.
//
// Architecture decision: RenderSettings.skybox cannot be set through an
// asset importer so we use a lightweight MonoBehaviour. Keeping it here
// in Utils makes it reusable and independent of all gameplay systems.

using UnityEngine;

namespace DeathMatch.Utils
{
    public class SkyboxApplier : MonoBehaviour
    {
        [SerializeField] private Material skyboxMaterial;

        private void Awake()
        {
            if (skyboxMaterial != null)
                RenderSettings.skybox = skyboxMaterial;
        }
    }
}
