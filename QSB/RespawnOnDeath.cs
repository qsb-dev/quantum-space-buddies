using OWML.ModHelper.Events;
using System.Linq;
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
        ShipComponent[] _shipComponents;

        void Awake()
        {
            GlobalMessenger.AddListener("WakeUp", PlayerWokeUp);

            Instance = this;
            QSB.Helper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(Patches), nameof(Patches.PreFinishDeathSequence));
        }

        void PlayerWokeUp()
        {
            _playerSpawner = FindObjectOfType<PlayerSpawner>();
            _fluidDetector = Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>();
            _playerResources = Locator.GetPlayerTransform().GetComponent<PlayerResources>();

            var shipTransform = Locator.GetShipTransform();
            if (shipTransform)
            {
                _shipComponents = Locator.GetShipTransform().GetComponentsInChildren<ShipComponent>();
                _shipBody = Locator.GetShipBody();
                _shipSpawnPoint = GetSpawnPoint(true);

                // Move debug spawn point to initial ship position.
                _playerSpawnPoint = GetSpawnPoint();
                _shipSpawnPoint.transform.position = shipTransform.position;
                _shipSpawnPoint.transform.rotation = shipTransform.rotation;
            }

        }

        public void ResetShip()
        {
            if (!_shipBody)
            {
                return;
            }

            // Reset ship position.
            _shipBody.SetVelocity(_shipSpawnPoint.GetPointVelocity());
            _shipBody.WarpToPositionRotation(_shipSpawnPoint.transform.position, _shipSpawnPoint.transform.rotation);

            // Reset ship damage.
            if (Locator.GetShipTransform())
            {
                for (int i = 0; i < _shipComponents.Length; i++)
                {
                    _shipComponents[i].SetDamaged(false);
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

            _playerResources.SetValue("_isSuffocating", false);

            // Reset player health and resources.
            _playerResources.DebugRefillResources();
        }

        SpawnPoint GetSpawnPoint(bool isShip = false)
        {
            return _playerSpawner
                .GetValue<SpawnPoint[]>("_spawnList")
                .FirstOrDefault(spawnPoint =>
                    spawnPoint.GetSpawnLocation() == SpawnLocation.TimberHearth && spawnPoint.IsShipSpawn() == isShip
                );
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
