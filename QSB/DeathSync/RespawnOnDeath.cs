using OWML.Common;
using OWML.Utils;
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

		private SpawnPoint _playerSpawnPoint;
		private PlayerSpawner _playerSpawner;
		private FluidDetector _fluidDetector;
		private PlayerResources _playerResources;
		private PlayerSpacesuit _spaceSuit;
		private SuitPickupVolume[] _suitPickupVolumes;

		public void Awake() => Instance = this;

		public void Init()
		{
			var playerTransform = Locator.GetPlayerTransform();
			_playerResources = playerTransform.GetComponent<PlayerResources>();
			_spaceSuit = Locator.GetPlayerSuit();
			_playerSpawner = FindObjectOfType<PlayerSpawner>();
			_suitPickupVolumes = FindObjectsOfType<SuitPickupVolume>();
			_fluidDetector = Locator.GetPlayerCamera().GetComponentInChildren<FluidDetector>();
			_playerSpawnPoint = GetSpawnPoint();
		}

		public void ResetPlayer()
		{
			DebugLog.DebugWrite($"Trying to reset player.");
			if (_playerSpawnPoint == null)
			{
				DebugLog.ToConsole("Warning - _playerSpawnPoint is null!", MessageType.Warning);
				Init();
			}

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

			foreach (var pickupVolume in _suitPickupVolumes)
			{
				var containsSuit = pickupVolume.GetValue<bool>("_containsSuit");
				var allowReturn = pickupVolume.GetValue<bool>("_allowSuitReturn");

				if (!containsSuit && allowReturn)
				{

					var interactVolume = pickupVolume.GetValue<MultipleInteractionVolume>("_interactVolume");
					var pickupSuitIndex = pickupVolume.GetValue<int>("_pickupSuitCommandIndex");

					pickupVolume.SetValue("_containsSuit", true);
					interactVolume.ChangePrompt(UITextType.SuitUpPrompt, pickupSuitIndex);

					var suitGeometry = pickupVolume.GetValue<GameObject>("_suitGeometry");
					var suitCollider = pickupVolume.GetValue<OWCollider>("_suitOWCollider");
					var toolGeometries = pickupVolume.GetValue<GameObject[]>("_toolGeometry");

					suitGeometry.SetActive(true);
					suitCollider.SetActivation(true);
					foreach (var geo in toolGeometries)
					{
						geo.SetActive(true);
					}
				}
			}
		}

		private SpawnPoint GetSpawnPoint()
		{
			var spawnList = _playerSpawner.GetValue<SpawnPoint[]>("_spawnList");
			if (spawnList == null)
			{
				DebugLog.ToConsole($"Warning - _spawnList was null for player spawner!", MessageType.Warning);
				return null;
			}

			return spawnList.FirstOrDefault(spawnPoint =>
					spawnPoint.GetSpawnLocation() == SpawnLocation.TimberHearth
					&& spawnPoint.IsShipSpawn() == false);
		}
	}
}