using OWML.Utils;
using QSB.Animation.Player;
using QSB.Events;
using QSB.Instruments;
using QSB.RoastingSync;
using QSB.SectorSync;
using QSB.Syncs.TransformSync;
using QSB.Tools;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Player.TransformSync
{
	public class PlayerTransformSync : SectoredTransformSync
	{
		static PlayerTransformSync() => AnimControllerPatch.Init();

		private Transform _visibleCameraRoot;
		private Transform _networkCameraRoot => gameObject.transform.GetChild(0);

		private Transform _visibleStickPivot;
		private Transform _networkStickPivot => gameObject.transform.GetChild(1);

		private Transform _visibleStickTip;
		private Transform _networkStickTip => _networkStickPivot.GetChild(0);

		private Transform GetStickPivot()
			=> Resources.FindObjectsOfTypeAll<RoastingStickController>().First().transform.Find("Stick_Root/Stick_Pivot");

		public override void OnStartLocalPlayer()
			=> LocalInstance = this;

		public override void Start()
		{
			base.Start();
			Player.TransformSync = this;
		}

		protected override void OnDestroy()
		{
			QSBPlayerManager.OnRemovePlayer?.Invoke(PlayerId);
			base.OnDestroy();
			if (QSBPlayerManager.PlayerExists(PlayerId))
			{
				Player.HudMarker?.Remove();
				QSBPlayerManager.RemovePlayer(PlayerId);
			}
		}

		private Transform GetPlayerModel() =>
			Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");

		protected override Transform InitLocalTransform()
		{
			SectorSync.Init(Locator.GetPlayerSectorDetector(), this);

			// player body
			var playerBody = GetPlayerModel();
			GetComponent<AnimationSync>().InitLocal(playerBody);
			GetComponent<InstrumentsManager>().InitLocal(playerBody);
			Player.Body = playerBody.gameObject;

			// camera
			var cameraBody = Locator.GetPlayerCamera().gameObject.transform;
			Player.Camera = Locator.GetPlayerCamera();
			Player.CameraBody = cameraBody.gameObject;
			_visibleCameraRoot = cameraBody;

			// stick
			var pivot = GetStickPivot();
			Player.RoastingStick = pivot.gameObject;
			_visibleStickPivot = pivot;
			_visibleStickTip = pivot.Find("Stick_Tip");

			Player.PlayerStates.IsReady = true;
			QSBEventManager.FireEvent(EventNames.QSBPlayerReady, true);
			DebugLog.DebugWrite("PlayerTransformSync init done - Request state!");
			QSBEventManager.FireEvent(EventNames.QSBPlayerStatesRequest);

			return playerBody;
		}

		protected override Transform InitRemoteTransform()
		{
			// player body
			var playerBody = Instantiate(GetPlayerModel());
			Player.Body = playerBody.gameObject;

			GetComponent<AnimationSync>().InitRemote(playerBody);
			GetComponent<InstrumentsManager>().InitRemote(playerBody);

			var marker = playerBody.gameObject.AddComponent<PlayerHUDMarker>();
			marker.Init(Player);

			playerBody.gameObject.AddComponent<PlayerMapMarker>().PlayerName = Player.Name;

			// camera
			var cameraBody = new GameObject("RemotePlayerCamera");
			cameraBody.transform.parent = playerBody;

			PlayerToolsManager.Init(cameraBody.transform);

			var camera = cameraBody.AddComponent<Camera>();
			camera.enabled = false;
			var owcamera = cameraBody.AddComponent<OWCamera>();
			owcamera.fieldOfView = 70;
			owcamera.nearClipPlane = 0.1f;
			owcamera.farClipPlane = 50000f;
			Player.Camera = owcamera;
			Player.CameraBody = cameraBody;
			_visibleCameraRoot = cameraBody.transform;

			// stick

			var newPivot = Instantiate(GetStickPivot());
			// TODO : this is meant to be the camera?
			newPivot.parent = null;
			newPivot.gameObject.SetActive(false);
			Destroy(newPivot.Find("Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm").gameObject);
			Destroy(newPivot.Find("Stick_Tip/Props_HEA_RoastingStick/RoastingStick_Arm_NoSuit").gameObject);
			var mallowRoot = newPivot.Find("Stick_Tip/Mallow_Root");
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

			Player.RoastingStick = newPivot.gameObject;
			Player.Marshmallow = newMarshmallow;
			mallowRoot.gameObject.SetActive(true);
			_visibleStickPivot = newPivot;
			_visibleStickTip = newPivot.Find("Stick_Tip");

			return playerBody;
		}

		protected override void UpdateTransform()
		{
			base.UpdateTransform();

			if (HasAuthority)
			{
				_networkStickPivot.localPosition = _visibleStickPivot.localPosition;
				_networkStickPivot.localRotation = _visibleStickPivot.localRotation;

				_networkStickTip.localPosition = _visibleStickTip.localPosition;
				_networkStickTip.localRotation = _visibleStickTip.localRotation;

				_networkCameraRoot.localPosition = _visibleCameraRoot.localPosition;
				_networkCameraRoot.localRotation = _visibleCameraRoot.localRotation;

				return;
			}

			_visibleStickPivot.localPosition = _networkStickPivot.localPosition;
			_visibleStickPivot.localRotation = _networkStickPivot.localRotation;

			_visibleStickTip.localPosition = _networkStickTip.localPosition;
			_visibleStickTip.localRotation = _networkStickTip.localRotation;

			_visibleCameraRoot.localPosition = _networkCameraRoot.localPosition;
			_visibleCameraRoot.localRotation = _networkCameraRoot.localRotation;
		}

		public override bool IsReady => Locator.GetPlayerTransform() != null
			&& Player != null
			&& QSBPlayerManager.PlayerExists(Player.PlayerId)
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U;

		public static PlayerTransformSync LocalInstance { get; private set; }

		public override bool UseInterpolation => true;

		public override TargetType Type => TargetType.Player;
	}
}