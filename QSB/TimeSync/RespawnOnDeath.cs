using System.Linq;
using OWML.ModHelper.Events;
using QSB.Events;
using QSB.Messaging;
using UnityEngine;

namespace QSB.TimeSync
{
    public class RespawnOnDeath : MonoBehaviour
    {
        private static RespawnOnDeath _instance;

        private SpawnPoint _shipSpawnPoint;
        private SpawnPoint _playerSpawnPoint;
        private OWRigidbody _shipBody;
        private PlayerSpawner _playerSpawner;
        private FluidDetector _fluidDetector;
        private PlayerResources _playerResources;
        private ShipComponent[] _shipComponents;
        private HatchController _hatchController;
        private ShipCockpitController _cockpitController;
        private PlayerSpacesuit _spaceSuit;

        private MessageHandler<DeathMessage> _deathHandler;

        private void Awake()
        {
            GlobalMessenger.AddListener("WakeUp", PlayerWokeUp);

            _instance = this;
            QSB.Helper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(Patches), nameof(Patches.PreFinishDeathSequence));
        }

        private void PlayerWokeUp()
        {
            var playerTransform = Locator.GetPlayerTransform();
            _playerResources = playerTransform.GetComponent<PlayerResources>();
            _spaceSuit = playerTransform.GetComponentInChildren<PlayerSpacesuit>(true);
            _playerSpawner = FindObjectOfType<PlayerSpawner>();
            _fluidDetector = Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>();

            var shipTransform = Locator.GetShipTransform();
            if (shipTransform != null)
            {
                _shipComponents = shipTransform.GetComponentsInChildren<ShipComponent>();
                _hatchController = shipTransform.GetComponentInChildren<HatchController>();
                _cockpitController = shipTransform.GetComponentInChildren<ShipCockpitController>();
                _shipBody = Locator.GetShipBody();
                _shipSpawnPoint = GetSpawnPoint(true);

                // Move debug spawn point to initial ship position.
                _playerSpawnPoint = GetSpawnPoint();
                _shipSpawnPoint.transform.position = shipTransform.position;
                _shipSpawnPoint.transform.rotation = shipTransform.rotation;
            }

            _deathHandler = new MessageHandler<DeathMessage>();
            _deathHandler.OnServerReceiveMessage += OnServerReceiveMessage;
            _deathHandler.OnClientReceiveMessage += OnClientReceiveMessage;
        }

        public void ResetShip()
        {
            if (_shipBody == null)
            {
                return;
            }

            // Reset ship position.
            _shipBody.SetVelocity(_shipSpawnPoint.GetPointVelocity());
            _shipBody.WarpToPositionRotation(_shipSpawnPoint.transform.position, _shipSpawnPoint.transform.rotation);

            // Reset ship damage.
            if (Locator.GetShipTransform())
            {
                foreach (var shipComponent in _shipComponents)
                {
                    shipComponent.SetDamaged(false);
                }
            }

            Invoke(nameof(ExitShip), 0.01f);
        }

        private void ExitShip()
        {
            _cockpitController.Invoke("ExitFlightConsole");
            _cockpitController.Invoke("CompleteExitFlightConsole");
            _hatchController.SetValue("_isPlayerInShip", false);
            _hatchController.Invoke("OpenHatch");
            GlobalMessenger.FireEvent("ExitShip");
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

            // Stop suffocation sound effect.
            _playerResources.SetValue("_isSuffocating", false);

            // Reset player health and resources.
            _playerResources.DebugRefillResources();

            // Remove space suit.
            _spaceSuit.RemoveSuit(true);
        }

        private SpawnPoint GetSpawnPoint(bool isShip = false)
        {
            return _playerSpawner
                .GetValue<SpawnPoint[]>("_spawnList")
                .FirstOrDefault(spawnPoint =>
                    spawnPoint.GetSpawnLocation() == SpawnLocation.TimberHearth && spawnPoint.IsShipSpawn() == isShip
                );
        }

        private void OnServerReceiveMessage(DeathMessage message)
        {
            _deathHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(DeathMessage message)
        {
            var playerName = PlayerJoin.PlayerNames.TryGetValue(message.SenderId, out var n) ? n : message.PlayerName;
            var deathType = ((DeathType)message.DeathId).ToString();
            DebugLog.All($"{playerName} was killed by {deathType}!");
        }

        internal static class Patches
        {
            public static bool PreFinishDeathSequence(DeathType deathType)
            {
                BroadcastDeath(deathType);

                if (deathType == DeathType.Supernova)
                {
                    // Allow real death
                    return true;
                }

                _instance.ResetShip();
                _instance.ResetPlayer();

                // Prevent original death method from running.
                return false;
            }

            private static void BroadcastDeath(DeathType deathType)
            {
                var message = new DeathMessage
                {
                    PlayerName = PlayerJoin.MyName,
                    SenderId = NetPlayer.LocalInstance.netId.Value,
                    DeathId = (short)deathType
                };
                _instance._deathHandler.SendToServer(message);
            }

        }
    }
}
