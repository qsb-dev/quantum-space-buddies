using OWML.ModHelper.Events;
using UnityEngine;

namespace QSB
{
    class RespawnOnDeath : MonoBehaviour
    {
        Vector3 _shipSpawnPosition;
        Quaternion _shipSpawnRotation;
        static RespawnOnDeath Instance;

        void Awake()
        {
            Instance = this;
            QSB.Helper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(Patches), nameof(Patches.PreFinishDeathSequence));

            _shipSpawnPosition = Locator.GetShipTransform().position;
            _shipSpawnRotation = Locator.GetShipTransform().rotation;
        }

        public void WaitAndResetShip()
        {
            Invoke(nameof(ResetShip), 1);
        }

        void ResetShip()
        {
            DebugLog.Screen("Reset Ship");
            Locator.GetShipBody().SetPosition(_shipSpawnPosition);
            Locator.GetShipBody().SetRotation(_shipSpawnRotation);
        }

        internal static class Patches
        {
            public static bool PreFinishDeathSequence()
            {
                // Teleport palyer to Timber Hearth.
                typeof(PlayerState).SetValue("_insideShip", false);
                var spawnPoint = FindObjectOfType<PlayerSpawner>().GetSpawnPoint(SpawnLocation.TimberHearth);
                OWRigidbody playerBody = Locator.GetPlayerBody();
                playerBody.WarpToPositionRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
                playerBody.SetVelocity(spawnPoint.GetPointVelocity());
                spawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerDetector().gameObject);
                spawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>().gameObject);
                spawnPoint.OnSpawnPlayer();

                // Reset fuel and health.
                Locator.GetPlayerTransform().GetComponent<PlayerResources>().DebugRefillResources();

                Instance.WaitAndResetShip();

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
