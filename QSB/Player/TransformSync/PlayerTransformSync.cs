using OWML.Utils;
using QSB.Animation.Player;
using QSB.Audio;
using QSB.Events;
using QSB.Instruments;
using QSB.RoastingSync;
using QSB.SectorSync;
using QSB.Syncs.Sectored.Transforms;
using QSB.Tools;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Player.TransformSync
{
	public class PlayerTransformSync : SectoredTransformSync
	{
		static PlayerTransformSync() => AnimControllerPatch.Init();

		public override bool IsPlayerObject => true;

		private Transform _visibleCameraRoot;
		private Transform _networkCameraRoot => gameObject.transform.GetChild(0);

		private Transform _visibleRoastingSystem;
		private Transform _networkRoastingSystem => gameObject.transform.GetChild(1);
		private Transform _networkStickRoot => _networkRoastingSystem.GetChild(0);

		private Transform _visibleStickPivot;
		private Transform _networkStickPivot => _networkStickRoot.GetChild(0);

		private Transform _visibleStickTip;
		private Transform _networkStickTip => _networkStickPivot.GetChild(0);

		protected Vector3 _cameraPositionVelocity;
		protected Quaternion _cameraRotationVelocity;
		protected Vector3 _pivotPositionVelocity;
		protected Quaternion _pivotRotationVelocity;
		protected Vector3 _tipPositionVelocity;
		protected Quaternion _tipRotationVelocity;
		protected Vector3 _roastingPositionVelocity;
		protected Quaternion _roastingRotationVelocity;

		private Transform GetStickPivot()
			=> QSBWorldSync.GetUnityObjects<RoastingStickController>().First().transform.Find("Stick_Root/Stick_Pivot");

		public override void OnStartLocalPlayer()
			=> LocalInstance = this;

		public override void Start()
		{
			base.Start();
			QSBPlayerManager.AddPlayer(PlayerId);
			Player.TransformSync = this;
		}

		protected override void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isInUniverse)
		{
			if (!HasAuthority)
			{
				base.OnSceneLoaded(oldScene, newScene, isInUniverse);
			}

			if (isInUniverse && !_isInitialized)
			{
				Player.IsReady = false;
				QSBEventManager.FireEvent(EventNames.QSBPlayerReady, false);
			}

			if (!isInUniverse)
			{
				Player.IsReady = false;
				QSBEventManager.FireEvent(EventNames.QSBPlayerReady, false);
			}

			base.OnSceneLoaded(oldScene, newScene, isInUniverse);
		}

		protected override void Init()
		{
			base.Init();

			Player.IsReady = true;
			QSBEventManager.FireEvent(EventNames.QSBPlayerReady, true);
		}

		protected override void OnDestroy()
		{
			// TODO : Maybe move this to a leave event...? Would ensure everything could finish up before removing the player
			QSBPlayerManager.OnRemovePlayer?.Invoke(PlayerId);
			base.OnDestroy();
			if (QSBPlayerManager.PlayerExists(PlayerId))
			{
				Player.HudMarker?.Remove();
				QSBPlayerManager.RemovePlayer(PlayerId);
			}
		}

		protected override Component InitLocalTransform()
		{
			QSBCore.UnityEvents.RunWhen(() => WorldObjectManager.AllObjectsReady, () => SectorSync.Init(Locator.GetPlayerSectorDetector(), TargetType.Player));

			// player body
			var player = Locator.GetPlayerTransform();
			var playerModel = player.Find("Traveller_HEA_Player_v2");
			GetComponent<AnimationSync>().InitLocal(playerModel);
			GetComponent<InstrumentsManager>().InitLocal(player);
			Player.Body = player.gameObject;

			// camera
			var cameraBody = Locator.GetPlayerCamera().gameObject.transform;
			Player.Camera = Locator.GetPlayerCamera();
			Player.CameraBody = cameraBody.gameObject;
			_visibleCameraRoot = cameraBody;

			PlayerToolsManager.InitLocal();

			// stick
			var pivot = GetStickPivot();
			Player.RoastingStick = pivot.parent.gameObject;
			_visibleRoastingSystem = pivot.parent.parent;
			_visibleStickPivot = pivot;
			_visibleStickTip = pivot.Find("Stick_Tip");

			QSBEventManager.FireEvent(EventNames.QSBRequestStateResync);

			return player;
		}

		protected override Component InitRemoteTransform()
		{
			/*
			 * CREATE PLAYER STRUCTURE
			 */

			// Variable naming convention is broken here to reflect OW unity project (with REMOTE_ prefixed) for readability

			var REMOTE_Player_Body = new GameObject("REMOTE_Player_Body");

			var REMOTE_PlayerCamera = new GameObject("REMOTE_PlayerCamera");
			REMOTE_PlayerCamera.transform.parent = REMOTE_Player_Body.transform;
			REMOTE_PlayerCamera.transform.localPosition = new Vector3(0f, 0.8496093f, 0.1500003f);

			var REMOTE_RoastingSystem = new GameObject("REMOTE_RoastingSystem");
			REMOTE_RoastingSystem.transform.parent = REMOTE_Player_Body.transform;
			REMOTE_RoastingSystem.transform.localPosition = new Vector3(0f, 0.4f, 0f);

			var REMOTE_Stick_Root = new GameObject("REMOTE_Stick_Root");
			REMOTE_Stick_Root.transform.parent = REMOTE_RoastingSystem.transform;
			REMOTE_Stick_Root.transform.localPosition = new Vector3(0.25f, 0f, 0.08f);
			REMOTE_Stick_Root.transform.localRotation = Quaternion.Euler(0f, -10f, 0f);

			/*
			 * SET UP PLAYER BODY
			 */

			var player = Locator.GetPlayerTransform();
			var playerModel = player.Find("Traveller_HEA_Player_v2");

			var REMOTE_Traveller_HEA_Player_v2 = Instantiate(playerModel);
			REMOTE_Traveller_HEA_Player_v2.transform.parent = REMOTE_Player_Body.transform;
			REMOTE_Traveller_HEA_Player_v2.transform.localPosition = new Vector3(0f, -1.03f, -0.2f);
			REMOTE_Traveller_HEA_Player_v2.transform.localRotation = Quaternion.Euler(-1.500009f, 0f, 0f);
			REMOTE_Traveller_HEA_Player_v2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

			Player.Body = REMOTE_Player_Body;

			GetComponent<AnimationSync>().InitRemote(REMOTE_Traveller_HEA_Player_v2);
			GetComponent<InstrumentsManager>().InitRemote(REMOTE_Player_Body.transform);

			var marker = REMOTE_Player_Body.AddComponent<PlayerHUDMarker>();
			marker.Init(Player);

			REMOTE_Player_Body.AddComponent<PlayerMapMarker>().PlayerName = Player.Name;

			Player.AudioController = PlayerAudioManager.InitRemote(REMOTE_Player_Body.transform);

			/*
			 * SET UP PLAYER CAMERA
			 */

			var camera = REMOTE_PlayerCamera.AddComponent<Camera>();
			camera.enabled = false;
			var owcamera = REMOTE_PlayerCamera.AddComponent<OWCamera>();
			owcamera.fieldOfView = 70;
			owcamera.nearClipPlane = 0.1f;
			owcamera.farClipPlane = 50000f;
			Player.Camera = owcamera;
			Player.CameraBody = REMOTE_PlayerCamera;
			_visibleCameraRoot = REMOTE_PlayerCamera.transform;

			PlayerToolsManager.InitRemote(Player);

			/*
			 * SET UP ROASTING STICK
			 */

			var REMOTE_Stick_Pivot = Instantiate(GetStickPivot());
			REMOTE_Stick_Pivot.name = "REMOTE_Stick_Pivot";
			REMOTE_Stick_Pivot.parent = REMOTE_Stick_Root.transform;
			REMOTE_Stick_Pivot.gameObject.SetActive(false);

			Destroy(REMOTE_Stick_Pivot.Find("Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm").gameObject);
			Destroy(REMOTE_Stick_Pivot.Find("Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm_NoSuit").gameObject);

			var mallowRoot = REMOTE_Stick_Pivot.Find("Stick_Tip/Mallow_Root");
			mallowRoot.gameObject.SetActive(false);
			var oldMarshmallow = mallowRoot.GetComponent<Marshmallow>();

			// Recreate particle system
			Destroy(mallowRoot.Find("MallowSmoke").GetComponent<RelativisticParticleSystem>());
			var newSystem = mallowRoot.Find("MallowSmoke").gameObject.AddComponent<CustomRelativisticParticleSystem>();
			newSystem.Init(Player);

			// Create new marshmallow
			var newMarshmallow = mallowRoot.gameObject.AddComponent<QSBMarshmallow>();
			newMarshmallow._fireRenderer = oldMarshmallow.GetValue<MeshRenderer>("_fireRenderer");
			newMarshmallow._smokeParticles = oldMarshmallow.GetValue<ParticleSystem>("_smokeParticles");
			newMarshmallow._mallowRenderer = oldMarshmallow.GetValue<MeshRenderer>("_mallowRenderer");
			newMarshmallow._rawColor = oldMarshmallow.GetValue<Color>("_rawColor");
			newMarshmallow._toastedColor = oldMarshmallow.GetValue<Color>("_toastedColor");
			newMarshmallow._burntColor = oldMarshmallow.GetValue<Color>("_burntColor");
			Destroy(oldMarshmallow);

			Player.RoastingStick = REMOTE_Stick_Pivot.gameObject;
			Player.Marshmallow = newMarshmallow;
			mallowRoot.gameObject.SetActive(true);
			_visibleRoastingSystem = REMOTE_RoastingSystem.transform;
			_visibleStickPivot = REMOTE_Stick_Pivot;
			_visibleStickTip = REMOTE_Stick_Pivot.Find("Stick_Tip");

			return REMOTE_Player_Body.transform;
		}

		protected override bool UpdateTransform()
		{
			if (!base.UpdateTransform())
			{
				return false;
			}

			UpdateSpecificTransform(_visibleStickPivot, _networkStickPivot, ref _pivotPositionVelocity, ref _pivotRotationVelocity);
			UpdateSpecificTransform(_visibleStickTip, _networkStickTip, ref _tipPositionVelocity, ref _tipRotationVelocity);
			UpdateSpecificTransform(_visibleCameraRoot, _networkCameraRoot, ref _cameraPositionVelocity, ref _cameraRotationVelocity);
			UpdateSpecificTransform(_visibleRoastingSystem, _networkRoastingSystem, ref _roastingPositionVelocity, ref _roastingRotationVelocity);
			return true;
		}

		private void UpdateSpecificTransform(Transform visible, Transform network, ref Vector3 positionVelocity, ref Quaternion rotationVelocity)
		{
			if (HasAuthority)
			{
				network.localPosition = visible.localPosition;
				network.localRotation = visible.localRotation;
				return;
			}

			visible.localPosition = Vector3.SmoothDamp(visible.localPosition, network.localPosition, ref positionVelocity, SmoothTime);
			visible.localRotation = QuaternionHelper.SmoothDamp(visible.localRotation, network.localRotation, ref rotationVelocity, SmoothTime);
		}

		protected override void OnRenderObject()
		{
			base.OnRenderObject();

			if (!WorldObjectManager.AllObjectsReady
				|| !QSBCore.ShowLinesInDebug
				|| !IsReady
				|| ReferenceTransform == null)
			{
				return;
			}

			Popcron.Gizmos.Cube(ReferenceTransform.TransformPoint(_networkRoastingSystem.position), ReferenceTransform.TransformRotation(_networkRoastingSystem.rotation), Vector3.one / 4, Color.red);
			Popcron.Gizmos.Cube(ReferenceTransform.TransformPoint(_networkStickPivot.position), ReferenceTransform.TransformRotation(_networkStickPivot.rotation), Vector3.one / 4, Color.red);
			Popcron.Gizmos.Cube(ReferenceTransform.TransformPoint(_networkStickTip.position), ReferenceTransform.TransformRotation(_networkStickTip.rotation), Vector3.one / 4, Color.red);
			Popcron.Gizmos.Cube(ReferenceTransform.TransformPoint(_networkCameraRoot.position), ReferenceTransform.TransformRotation(_networkCameraRoot.rotation), Vector3.one / 4, Color.red);

			Popcron.Gizmos.Cube(_visibleRoastingSystem.position, _visibleRoastingSystem.rotation, Vector3.one / 4, Color.magenta);
			Popcron.Gizmos.Cube(_visibleStickPivot.position, _visibleStickPivot.rotation, Vector3.one / 4, Color.blue);
			Popcron.Gizmos.Cube(_visibleStickTip.position, _visibleStickTip.rotation, Vector3.one / 4, Color.yellow);
			Popcron.Gizmos.Cube(_visibleCameraRoot.position, _visibleCameraRoot.rotation, Vector3.one / 4, Color.grey);
		}

		public override bool IsReady
			=> Locator.GetPlayerTransform() != null;

		public static PlayerTransformSync LocalInstance { get; private set; }

		public override bool UseInterpolation => true;
	}
}
