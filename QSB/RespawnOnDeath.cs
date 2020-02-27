﻿using OWML.ModHelper.Events;
using UnityEngine;

namespace QSB
{
    class RespawnOnDeath : MonoBehaviour
    {
        SpawnPoint _shipSpawnPoint;
        static RespawnOnDeath Instance;
        OWRigidbody _shipBody;
        PlayerSpawner _playerSpawner;

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

            _shipSpawnPoint.transform.position = Locator.GetShipTransform().position;
            _shipSpawnPoint.transform.rotation = Locator.GetShipTransform().rotation;

            _shipBody = Locator.GetShipBody();
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

        public void ResetShip()
        {
            _shipBody.SetVelocity(_shipSpawnPoint.GetPointVelocity());
            _shipBody.WarpToPositionRotation(_shipSpawnPoint.transform.position, _shipSpawnPoint.transform.rotation);
        }

        internal static class Patches
        {
            public static bool PreFinishDeathSequence()
            {
                // Teleport palyer to Timber Hearth.
                typeof(PlayerState).SetValue("_insideShip", false);
                var spawnPoint = Instance.GetSpawnPoint();
                OWRigidbody playerBody = Locator.GetPlayerBody();
                playerBody.WarpToPositionRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
                playerBody.SetVelocity(spawnPoint.GetPointVelocity());
                spawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerDetector().gameObject);
                spawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>().gameObject);
                spawnPoint.OnSpawnPlayer();

                // Reset fuel and health.
                Locator.GetPlayerTransform().GetComponent<PlayerResources>().DebugRefillResources();

                Instance.ResetShip();

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
