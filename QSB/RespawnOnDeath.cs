using OWML.ModHelper.Events;
using UnityEngine;

namespace QSB
{
    class RespawnOnDeath : MonoBehaviour
    {
        static RespawnOnDeath Instance;

        SpawnPoint _shipSpawnPoint;
        SpawnPoint _playerSpawnPoint;
        OWRigidbody _shipBody;
        PlayerSpawner _playerSpawner;
        FluidDetector _fluidDetector;
        PlayerResources _playerResources;

        void Awake()
        {
            GlobalMessenger.AddListener("WakeUp", PlayerWokeUp);

            Instance = this;
            QSB.Helper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(Patches), nameof(Patches.PreFinishDeathSequence));
        }

        void PlayerWokeUp()
        {
            _playerSpawner = FindObjectOfType<PlayerSpawner>();
            _shipSpawnPoint = GetSpawnPoint(true);
            _fluidDetector = Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>();
            _playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
            _shipBody = Locator.GetShipBody();

            // Move debug spawn point to initial ship position.
            _playerSpawnPoint = GetSpawnPoint();
            _shipSpawnPoint.transform.position = Locator.GetShipTransform().position;
            _shipSpawnPoint.transform.rotation = Locator.GetShipTransform().rotation;
        }

        public void ResetShip()
        {
            // Reset ship position.
            _shipBody.SetVelocity(_shipSpawnPoint.GetPointVelocity());
            _shipBody.WarpToPositionRotation(_shipSpawnPoint.transform.position, _shipSpawnPoint.transform.rotation);

            // Reset ship damage.
            if (Locator.GetShipTransform())
            {
                ShipComponent[] shipComponents = Locator.GetShipTransform().GetComponentsInChildren<ShipComponent>();
                for (int i = 0; i < shipComponents.Length; i++)
                {
                    shipComponents[i].SetDamaged(false);
                }
            }
        }

        public void ResetPlayer()
        {
            // Reset player position.
            OWRigidbody playerBody = Locator.GetPlayerBody();
            playerBody.WarpToPositionRotation(_playerSpawnPoint.transform.position, _playerSpawnPoint.transform.rotation);
            playerBody.SetVelocity(_playerSpawnPoint.GetPointVelocity());
            _playerSpawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerDetector().gameObject);
            _playerSpawnPoint.AddObjectToTriggerVolumes(_fluidDetector.gameObject);
            _playerSpawnPoint.OnSpawnPlayer();

            // Reset player health and resources.
            _playerResources.DebugRefillResources();
        }

        SpawnPoint GetSpawnPoint(bool isShip = false)
        {
            var spawnList = _playerSpawner.GetValue<SpawnPoint[]>("_spawnList");
            for (int i = 0; i < spawnList.Length; i++)
            {
                SpawnPoint spawnPoint = spawnList[i];
                if (spawnPoint.GetSpawnLocation() == SpawnLocation.TimberHearth && spawnPoint.IsShipSpawn() == isShip)
                {
                    return spawnPoint;
                }
            }
            return null;
        }

        internal static class Patches
        {
            public static bool PreFinishDeathSequence()
            {
                Instance.ResetPlayer();
                Instance.ResetShip();

                // Prevent original death method from running.
                return false;
            }
        }
    }
}
