using OWML.ModHelper.Events;
using UnityEngine;

namespace QSB
{
    class RespawnOnDeath : MonoBehaviour
    {
        SpawnPoint _shipSpawnPoint;
        static RespawnOnDeath Instance;

        void Awake()
        {
            GlobalMessenger.AddListener("WakeUp", PlayerWokeUp);

            Instance = this;
            QSB.Helper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(Patches), nameof(Patches.PreFinishDeathSequence));
        }

        void PlayerWokeUp()
        {
            typeof(PlayerState).SetValue("_insideShip", true);
            _shipSpawnPoint = FindObjectOfType<PlayerSpawner>().GetSpawnPoint(SpawnLocation.TimberHearth);
            typeof(PlayerState).SetValue("_insideShip", false);

            _shipSpawnPoint.transform.position = Locator.GetShipTransform().position;
            _shipSpawnPoint.transform.rotation = Locator.GetShipTransform().rotation;
        }

        public void WaitAndResetShip()
        {
            Invoke(nameof(ResetShip), 1);
        }

        void ResetShip()
        {
            Locator.GetShipBody().SetVelocity(_shipSpawnPoint.GetPointVelocity());
            Locator.GetShipBody().WarpToPositionRotation(_shipSpawnPoint.transform.position, _shipSpawnPoint.transform.rotation);
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
