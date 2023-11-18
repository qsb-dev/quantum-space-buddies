using QSB.Animation.Player;
using QSB.Audio;
using QSB.EchoesOfTheEye.LightSensorSync;
using QSB.Player;
using QSB.RoastingSync;
using QSB.Tools;
using QSB.Utility;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace QSB.PlayerBodySetup.Remote;

public static class RemotePlayerCreation
{
	private static GameObject _prefab;

	private static GameObject GetPrefab()
	{
		if (_prefab != null)
		{
			return _prefab;
		}

		_prefab = QSBCore.NetworkAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/REMOTE_Player_Body.prefab");
		ShaderReplacer.ReplaceShaders(_prefab);
		FontReplacer.ReplaceFonts(_prefab);
		QSBDopplerFixer.AddDopplerFixers(_prefab);
		return _prefab;
	}

	public static Transform CreatePlayer(
		PlayerInfo player,
		out Transform visibleCameraRoot,
		out Transform visibleRoastingSystem,
		out Transform visibleStickPivot,
		out Transform visibleStickTip)
	{
		/*
		 * CREATE PLAYER STRUCTURE
		 */

		// Variable naming convention is broken here to reflect OW unity project (with REMOTE_ prefixed) for readability

		var REMOTE_Player_Body = Object.Instantiate(GetPrefab());
		var REMOTE_PlayerDetector = REMOTE_Player_Body.transform.Find("REMOTE_PlayerDetector");
		var REMOTE_PlayerCamera = REMOTE_Player_Body.transform.Find("REMOTE_PlayerCamera").gameObject;
		var REMOTE_RoastingSystem = REMOTE_Player_Body.transform.Find("REMOTE_RoastingSystem").gameObject;
		var REMOTE_Stick_Root = REMOTE_RoastingSystem.transform.Find("REMOTE_Stick_Root").gameObject;
		var REMOTE_Traveller_HEA_Player_v2 = REMOTE_Player_Body.transform.Find("REMOTE_Traveller_HEA_Player_v2").gameObject;

		/*
		 * SET UP PLAYER BODY
		 */

		player.Body = REMOTE_Player_Body;
		player.ThrusterLightTracker = player.Body.GetComponentInChildren<ThrusterLightTracker>();
		player.FluidDetector = REMOTE_PlayerDetector.GetComponent<RemotePlayerFluidDetector>();
		player.RulesetDetector = REMOTE_PlayerDetector.GetComponent<RemotePlayerRulesetDetector>();
		player.HelmetAnimator = REMOTE_Traveller_HEA_Player_v2.GetComponent<HelmetAnimator>();

		player.AnimationSync.InitRemote(REMOTE_Traveller_HEA_Player_v2.transform);

		REMOTE_Player_Body.GetComponent<PlayerHUDMarker>().Init(player);
		REMOTE_Player_Body.GetComponent<PlayerMapMarker>().Init(player);
		player._ditheringAnimator = REMOTE_Player_Body.GetComponent<QSBDitheringAnimator>();
		player.DreamWorldSpawnAnimator = REMOTE_Player_Body.GetComponent<DreamWorldSpawnAnimator>();
		player.AudioController = REMOTE_Player_Body.transform.Find("REMOTE_Audio_Player").GetComponent<QSBPlayerAudioController>();

		/*
		 * SET UP PLAYER CAMERA
		 */

		var remoteCamera = REMOTE_PlayerCamera.GetComponent<Camera>();
		remoteCamera.enabled = false;
		remoteCamera.cullingMask = Locator.GetPlayerCamera().cullingMask;
		remoteCamera.farClipPlane = Locator.GetPlayerCamera().farClipPlane;
		remoteCamera.nearClipPlane = Locator.GetPlayerCamera().nearClipPlane;
		var owcamera = REMOTE_PlayerCamera.GetComponent<OWCamera>();
		player.Camera = owcamera;
		player.CameraBody = REMOTE_PlayerCamera;
		visibleCameraRoot = REMOTE_PlayerCamera.transform;
		//REMOTE_PlayerCamera.GetComponent<PostProcessingBehaviour>().profile = Locator.GetPlayerCamera().postProcessing.profile;
		//REMOTE_PlayerCamera.GetComponent<PlanetaryFogImageEffect>().fogShader = Locator.GetPlayerCamera().planetaryFog.fogShader;
		//REMOTE_PlayerCamera.GetComponent<FlashbackScreenGrabImageEffect>()._downsampleShader = Locator.GetPlayerCamera().GetComponent<FlashbackScreenGrabImageEffect>()._downsampleShader;

		player.LightSensor.gameObject.GetAddComponent<QSBPlayerLightSensor>();

		PlayerToolsManager.InitRemote(player);

		/*
		 * SET UP ROASTING STICK
		 */

		var REMOTE_Stick_Pivot = REMOTE_Stick_Root.transform.GetChild(0);
		var mallowRoot = REMOTE_Stick_Pivot.Find("REMOTE_Stick_Tip/Mallow_Root");
		var newSystem = mallowRoot.Find("MallowSmoke").gameObject.GetComponent<CustomRelativisticParticleSystem>();
		newSystem.Init(player);
		player.RoastingStick = REMOTE_Stick_Pivot.gameObject;
		var marshmallow = mallowRoot.GetComponent<QSBMarshmallow>();
		player.Marshmallow = marshmallow;

		visibleRoastingSystem = REMOTE_RoastingSystem.transform;
		visibleStickPivot = REMOTE_Stick_Pivot;
		visibleStickTip = REMOTE_Stick_Pivot.Find("REMOTE_Stick_Tip");

		return REMOTE_Player_Body.transform;
	}
}