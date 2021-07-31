using OWML.Common;
using OWML.Utils;
using QSB.Events;
using QSB.Player;
using QSB.Player.TransformSync;
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
		private Vector3 _deathPositionRelative;

		public Transform DeathClosestAstroObject { get; private set; }
		public Vector3 DeathPositionWorld
			=> DeathClosestAstroObject.TransformPoint(_deathPositionRelative);
		public Vector3 DeathPlayerUpVector { get; private set; }
		public Vector3 DeathPlayerForwardVector { get; private set; }

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
			DebugLog.DebugWrite($"RESET PLAYER");
			if (_playerSpawnPoint == null)
			{
				DebugLog.ToConsole("Warning - _playerSpawnPoint is null!", MessageType.Warning);
				Init();
			}

			var deadPlayersCount = QSBPlayerManager.PlayerList.Count(x => x.IsDead);

			if (deadPlayersCount == QSBPlayerManager.PlayerList.Count)
			{
				QSBEventManager.FireEvent(EventNames.QSBEndLoop, EndLoopReason.AllPlayersDead);
				return;
			}

			RespawnManager.Instance.TriggerRespawnMap();

			var inSpace = PlayerTransformSync.LocalInstance.SectorSync.SectorList.Count == 0;

			if (inSpace)
			{
				DeathClosestAstroObject = Locator.GetAstroObject(AstroObject.Name.Sun).transform;
			}
			else
			{
				var allAstroobjects = Resources.FindObjectsOfTypeAll<AstroObject>().Where(x => x.GetAstroObjectName() != AstroObject.Name.None && x.GetAstroObjectType() != AstroObject.Type.Satellite);
				var ordered = allAstroobjects.OrderBy(x => Vector3.SqrMagnitude(x.transform.position));
				DeathClosestAstroObject = ordered.First().transform;
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