﻿using OWML.Common;
using QSB.Localization;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.RespawnSync;
using QSB.ShipSync;
using QSB.Spectate;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.DeathSync;

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
	private GUIStyle _deadTextStyle;

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
		_deadTextStyle = new();
		_deadTextStyle.font = (Font)Resources.Load(@"fonts\english - latin\SpaceMono-Regular_Dynamic");
		_deadTextStyle.alignment = TextAnchor.MiddleCenter;
		_deadTextStyle.normal.textColor = Color.white;
		_deadTextStyle.fontSize = 20;
	}

	public void KillPlayer()
	{
		DebugLog.DebugWrite($"KILL PLAYER");
		if (_playerSpawnPoint == null)
		{
			DebugLog.ToConsole("Warning - _playerSpawnPoint is null!", MessageType.Warning);
			Init();
		}

		SpectateManager.Instance.TriggerSpectate();

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
		// death by oxygen turns this off, so we gotta enable it again
		Delay.RunNextFrame(() => _playerResources.enabled = true);
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

		QSBPlayerManager.LocalPlayer.LocalFlashlight.TurnOff(false);
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

	void OnGUI()
	{
		if (PlayerTransformSync.LocalInstance == null || ShipManager.Instance.ShipCockpitUI == null)
		{
			return;
		}

		if (QSBPlayerManager.LocalPlayer.IsDead)
		{
			GUI.contentColor = Color.white;

			var width = 200;
			var height = 100;

			// it is good day to be not dead

			var secondText = ShipManager.Instance.IsShipWrecked
				? string.Format(QSBLocalization.Current.WaitingForAllToDie, QSBPlayerManager.PlayerList.Count(x => !x.IsDead))
				: QSBLocalization.Current.WaitingForRespawn;

			GUI.Label(
				new Rect((Screen.width / 2) - (width / 2), (Screen.height / 2) - (height / 2) + (height * 2), width, height),
				$"SPECTATING [{SpectateManager.Instance.SpectateTarget.Name}]\n\n{QSBLocalization.Current.YouAreDead}\n{secondText}\nUSE ARROW KEYS TO CHANGE SPECTATE TARGET",
				_deadTextStyle);
		}
	}
}