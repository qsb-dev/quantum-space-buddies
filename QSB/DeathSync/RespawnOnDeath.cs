using OWML.Common;
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

	public readonly DeathType[] AllowedDeathTypes =
	{
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
		DebugLog.DebugWrite($"RESET PLAYER");
		if (_playerSpawnPoint == null)
		{
			DebugLog.ToConsole("Warning - _playerSpawnPoint is null!", MessageType.Warning);
			Init();
		}

		SpectateManager.Instance.TriggerSpectate();

		SetupDeathPositions();

		// do some exit dream world stuff since real deaths dont do that
		if (PlayerState.InDreamWorld())
		{
			ResetPlayerDreamworld();
		}

		ResetCloak();
		ResetPlayerComponents();
		ResetCanvases();

		var mixer = Locator.GetAudioMixer();
		mixer._deathMixed = false;
		mixer._nonEndTimesVolume.FadeTo(1, 0.5f);
		mixer._endTimesVolume.FadeTo(1, 0.5f);
		mixer.MixMap();

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

		ResetSuitState();

		QSBPlayerManager.LocalPlayer.LocalFlashlight.TurnOff(false);
	}

	private void SetupDeathPositions()
	{
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
	}

	private void ResetPlayerDreamworld()
	{
		var __instance = Locator.GetDreamWorldController();

		var wakeType = DreamWakeType.Default; // TODO maybe get actual death type? idk
		__instance._wakeType = wakeType;
		__instance.CheckDreamZone2Completion();
		__instance.CheckSleepWakeDieAchievement(wakeType);

		__instance._activeGhostGrabController?.ReleasePlayer();
		__instance._activeZoomPoint?.CancelZoom();

		if (__instance._outsideLanternBounds)
		{
			__instance.EnterLanternBounds();
		}

		__instance._simulationCamera.OnExitDreamWorld();
		SunLightController.UnregisterSunOverrider(__instance);
		if (__instance._proxyShadowLight != null)
		{
			__instance._proxyShadowLight.enabled = true;
		}
		__instance._insideDream = false;
		__instance._waitingToLightLantern = false;
		__instance._playerLantern.OnExitDreamWorld();

		// TODO : drop player lantern at campfire

		Locator.GetPlayerSectorDetector().RemoveFromAllSectors();

		__instance._playerLantern.OnExitDreamWorld();
		__instance._dreamArrivalPoint.OnExitDreamWorld();
		__instance._dreamCampfire.OnDreamCampfireExtinguished -= __instance.OnDreamCampfireExtinguished;
		__instance._dreamCampfire = null;

		__instance.ExtinguishDreamRaft();
		Locator.GetAudioMixer().UnmixDreamWorld();
		Locator.GetAudioMixer().UnmixSleepAtCampfire(1f);

		if (__instance._playerCamAmbientLightRenderer != null)
		{
			__instance._playerCamAmbientLightRenderer.enabled = false;
		}

		__instance._playerCamera.cullingMask |= 1 << LayerMask.NameToLayer("Sun");
		__instance._playerCamera.farClipPlane = __instance._prevPlayerCameraFarPlaneDist;
		__instance._prevPlayerCameraFarPlaneDist = 0f;
		__instance._playerCamera.mainCamera.backgroundColor = Color.black;
		__instance._playerCamera.planetaryFog.enabled = true;
		__instance._playerCamera.postProcessingSettings.screenSpaceReflectionAvailable = false;
		__instance._playerCamera.postProcessingSettings.ambientOcclusionAvailable = true;

		GlobalMessenger.FireEvent("ExitDreamWorld");
	}

	private void ResetCanvases()
	{
		foreach (var item in QSBWorldSync.GetUnityObjects<ScreenPromptList>())
		{
			item.OnPlayerResurrection();
		}

		foreach (var item in QSBWorldSync.GetUnityObjects<ThrustAndAttitudeIndicator>())
		{
			item.enabled = true;
		}

		foreach (var item in QSBWorldSync.GetUnityObjects<HUDCanvas>())
		{
			item.enabled = true;
		}

		foreach (var item in QSBWorldSync.GetUnityObjects<ReferenceFrameGUI>())
		{
			item.enabled = true;
			item._activeCam = Locator.GetMapController()._mapCamera;
			item._isMapView = true;
		}

		foreach (var item in QSBWorldSync.GetUnityObjects<AutopilotGUI>())
		{
			item.enabled = true;
		}
	}

	private void ResetSuitState()
	{
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

	private void ResetCloak()
	{
		if (!QSBCore.DLCInstalled)
		{
			return;
		}

		var cloak = Locator.GetCloakFieldController();
		// visible stranger disables cloak
		if (cloak)
		{
			cloak._playerInsideCloak = false;
			cloak._playerCloakFactor = 0f;
			cloak._worldFadeFactor = 0f;
			cloak._interiorRevealFactor = 0f;
			cloak._rendererFade = 1;
			cloak.OnPlayerExit.Invoke();
		}
		GlobalMessenger.FireEvent("ExitCloak");
	}

	private void ResetPlayerComponents()
	{
		var sectorList = PlayerTransformSync.LocalInstance.SectorDetector.SectorList;
		if (sectorList.All(x => x.Type != Sector.Name.TimberHearth))
		{
			// Spooky scary legacy code?
			// Original comment was "stops sectors from breaking when you die on TH??"
			// I think dying on TH used to break all the sectors. Something about you not technically re-entering TH when dying inside it.
			// I commented out these lines, and everything seemed fine. But I'm not gonna touch them just in case. :P
			Locator.GetPlayerSectorDetector().RemoveFromAllSectors();
			Locator.GetPlayerCameraDetector().GetComponent<AudioDetector>().DeactivateAllVolumes(0f);
		}

		PlayerState._isDead = false;
		Locator.GetPlayerController().OnPlayerResurrection();
		QSBWorldSync.GetUnityObject<PlayerBreathingAudio>().enabled = true;
		Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().OnPlayerResurrection();
		Locator.GetPlayerAudioController().OnPlayerResurrection();
		Locator.GetDeathManager()._isDying = false;

		var visorEffect = QSBWorldSync.GetUnityObject<VisorEffectController>();
		visorEffect._cracked = false;
		visorEffect._crackStartTime = 0f;
		visorEffect._crackEffectRenderer.enabled = false;
		visorEffect._crackEffectRenderer.material.SetFloat(visorEffect._propID_Cutoff, 1f);
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
				$"{QSBLocalization.Current.YouAreDead}\n{secondText}",
				_deadTextStyle);
		}
	}
}
