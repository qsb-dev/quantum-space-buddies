using OWML.Common;
using OWML.Utils;
using QSB.Events;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.DeathSync
{
	public class RespawnOnDeath : MonoBehaviour
	{
		public static RespawnOnDeath Instance;

		public readonly DeathType[] AllowedDeathTypes = {
			DeathType.BigBang,
			DeathType.Supernova,
			DeathType.TimeLoop
		};

		private readonly Vector3 ShipContainerOffset = new Vector3(-16.45f, -52.67f, 227.39f);
		private readonly Quaternion ShipContainerRotation = Quaternion.Euler(-76.937f, 1.062f, -185.066f);

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
		private ShipTractorBeamSwitch _shipTractorBeam;

		public void Awake() => Instance = this;

		public void Init()
		{
			DebugLog.DebugWrite($"init");
			var playerTransform = Locator.GetPlayerTransform();
			_playerResources = playerTransform.GetComponent<PlayerResources>();
			_spaceSuit = Locator.GetPlayerSuit();
			_playerSpawner = FindObjectOfType<PlayerSpawner>();
			_shipTractorBeam = FindObjectOfType<ShipTractorBeamSwitch>();
			_fluidDetector = Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>();

			_playerSpawnPoint = GetSpawnPoint();
			_shipSpawnPoint = GetSpawnPoint(true);

			var shipTransform = Locator.GetShipTransform();
			if (shipTransform == null)
			{
				DebugLog.ToConsole($"Warning - Init() ran when ship was null?", MessageType.Warning);
				return;
			}
			_shipComponents = shipTransform.GetComponentsInChildren<ShipComponent>();
			_hatchController = shipTransform.GetComponentInChildren<HatchController>();
			_cockpitController = shipTransform.GetComponentInChildren<ShipCockpitController>();
			_shipBody = Locator.GetShipBody();

			if (_shipSpawnPoint == null)
			{
				DebugLog.ToConsole("Warning - _shipSpawnPoint is null in Init()!", MessageType.Warning);
				return;
			}

			// Move debug spawn point to initial ship position (so ship doesnt spawn in space!)
			var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
			_shipSpawnPoint.transform.SetParent(timberHearth);
			_shipSpawnPoint.transform.localPosition = ShipContainerOffset;
			_shipSpawnPoint.transform.localRotation = ShipContainerRotation;
		}

		public void ResetPlayer()
		{
			if (_playerSpawnPoint == null)
			{
				DebugLog.ToConsole("Warning - _playerSpawnPoint is null!", MessageType.Warning);
				Init();
			}
			DebugLog.DebugWrite($"reset player");
			// Cant use _playerSpawner.DebugWarp because that will warp the ship if the player is in it
			var playerBody = Locator.GetPlayerBody();
			playerBody.WarpToPositionRotation(_playerSpawnPoint.transform.position, _playerSpawnPoint.transform.rotation);
			playerBody.SetVelocity(_playerSpawnPoint.GetPointVelocity());
			_playerSpawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerDetector().gameObject);
			_playerSpawnPoint.AddObjectToTriggerVolumes(_fluidDetector.gameObject);
			_playerSpawnPoint.OnSpawnPlayer();

			_playerResources.SetValue("_isSuffocating", false);
			_playerResources.DebugRefillResources();
			_spaceSuit.RemoveSuit(true);
		}

		public void ResetShip()
		{
			if (_shipSpawnPoint == null)
			{
				DebugLog.ToConsole("Warning - _shipSpawnPoint is null!", MessageType.Warning);
				Init();
			}

			if (_shipBody == null)
			{
				DebugLog.ToConsole($"Warning - Tried to reset ship, but the ship is null!", MessageType.Warning);
				return;
			}
			DebugLog.DebugWrite($"reset ship");

			_shipBody.SetVelocity(_shipSpawnPoint.GetPointVelocity());
			_shipBody.WarpToPositionRotation(_shipSpawnPoint.transform.position, _shipSpawnPoint.transform.rotation);

			foreach (var shipComponent in _shipComponents)
			{
				shipComponent.SetDamaged(false);
			}

			Invoke(nameof(ExitShip), 0.01f);
		}

		private void ExitShip()
		{
			_cockpitController.Invoke("ExitFlightConsole");
			_cockpitController.Invoke("CompleteExitFlightConsole");
			_hatchController.SetValue("_isPlayerInShip", false);
			_hatchController.Invoke("OpenHatch");
			_shipTractorBeam.ActivateTractorBeam();
		}

		private SpawnPoint GetSpawnPoint(bool isShip = false) 
			=> _playerSpawner
				.GetValue<SpawnPoint[]>("_spawnList")
				.FirstOrDefault(spawnPoint =>
					spawnPoint.GetSpawnLocation() == SpawnLocation.TimberHearth 
					&& spawnPoint.IsShipSpawn() == isShip);
	}
}