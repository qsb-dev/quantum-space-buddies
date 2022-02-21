using OWML.Common;
using QSB.Player.TransformSync;
using QSB.RespawnSync;
using QSB.Utility;
using QSB.WorldSync;
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
		private Vector3 _deathPositionRelative;

		public Transform DeathClosestAstroObject { get; private set; }
		public Vector3 DeathPositionWorld
			=> DeathClosestAstroObject == null
					? Vector3.zero
					: DeathClosestAstroObject.TransformPoint(_deathPositionRelative);
		public Vector3 DeathPlayerUpVector { get; private set; }
		public Vector3 DeathPlayerForwardVector { get; private set; }

		public void Awake() => Instance = this;

		public void Init()
		{
			DebugLog.DebugWrite($"INIT");
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
			DebugLog.DebugWrite($"RESET PLAYER");
			if (_playerSpawnPoint == null)
			{
				DebugLog.ToConsole("Warning - _playerSpawnPoint is null!", MessageType.Warning);
				Init();
			}

			RespawnManager.Instance.TriggerRespawnMap();

			var inSpace = PlayerTransformSync.LocalInstance.SectorDetector.SectorList.Count == 0;

			if (inSpace)
			{
				DeathClosestAstroObject = Locator.GetAstroObject(AstroObject.Name.Sun).transform;
			}
			else
			{
				var allAstroobjects = QSBWorldSync.GetUnityObjects<AstroObject>().Where(x => x.GetAstroObjectName() != AstroObject.Name.None && x.GetAstroObjectType() != AstroObject.Type.Satellite);
				var closest = allAstroobjects.MinBy(x => Vector3.SqrMagnitude(x.transform.position));
				DeathClosestAstroObject = closest.transform;
			}

			var deathPosition = Locator.GetPlayerTransform().position;
			_deathPositionRelative = DeathClosestAstroObject.InverseTransformPoint(deathPosition);
			DeathPlayerUpVector = Locator.GetPlayerTransform().up;
			DeathPlayerForwardVector = Locator.GetPlayerTransform().forward;

			var playerBody = Locator.GetPlayerBody();
			playerBody.WarpToPositionRotation(_playerSpawnPoint.transform.position, _playerSpawnPoint.transform.rotation);
			playerBody.SetVelocity(_playerSpawnPoint.GetPointVelocity());
			_playerSpawnPoint.AddObjectToTriggerVolumes(Locator.GetPlayerDetector().gameObject);
			_playerSpawnPoint.AddObjectToTriggerVolumes(_fluidDetector.gameObject);
			_playerSpawnPoint.OnSpawnPlayer();

			_playerResources._isSuffocating = false;
			_playerResources.DebugRefillResources();
			_spaceSuit.RemoveSuit(true);

			foreach (var pickupVolume in _suitPickupVolumes)
			{
				if (!pickupVolume._containsSuit && pickupVolume._allowSuitReturn)
				{
					pickupVolume._containsSuit = true;
					pickupVolume._interactVolume.ChangePrompt(UITextType.SuitUpPrompt, pickupVolume._pickupSuitCommandIndex);
					pickupVolume._suitGeometry.SetActive(true);
					pickupVolume._suitOWCollider.SetActivation(true);
					foreach (var geo in pickupVolume._toolGeometry)
					{
						geo.SetActive(true);
					}
				}
			}
		}

		private SpawnPoint GetSpawnPoint()
		{
			var spawnList = _playerSpawner._spawnList;
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
