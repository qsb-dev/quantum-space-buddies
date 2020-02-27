using OWML.ModHelper.Events;
using UnityEngine;

namespace QSB
{
    class RespawnOnDeath : MonoBehaviour
    {
        void Awake()
        {
            QSB.Helper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(Patches), nameof(Patches.PreFinishDeathSequence));
        }

        internal static class Patches
        {
            public static bool PreFinishDeathSequence()
            {
                // Teleport to Timber Hearth.
                var playerSpawner = GameObject.FindObjectOfType<PlayerSpawner>();
                playerSpawner.DebugWarp(playerSpawner.GetSpawnPoint(SpawnLocation.TimberHearth));

                // Reset fuel and health.
                Locator.GetPlayerTransform().GetComponent<PlayerResources>().DebugRefillResources();

                // Reset ship damage.
                if (Locator.GetShipTransform())
                {
                    ShipComponent[] shipComponents = Locator.GetShipTransform().GetComponentsInChildren<ShipComponent>();
                    for (int l = 0; l < shipComponents.Length; l++)
                    {
                        shipComponents[l].SetDamaged(false);
                    }
                }

                // Prevent original death method from running.
                return false;
            }
        }
    }
}
