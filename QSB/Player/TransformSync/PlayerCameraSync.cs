using QSB.Events;
using QSB.Tools;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine;

namespace QSB.Player.TransformSync
{
	public class PlayerCameraSync : SectoredTransformSync
	{
		protected override GameObject InitLocalTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetPlayerSectorDetector());
			var body = Locator.GetPlayerCamera().gameObject.transform;

			Player.Camera = Locator.GetPlayerCamera();
			Player.CameraBody = body.gameObject;

			Player.PlayerStates.IsReady = true;
			QSBEventManager.FireEvent(EventNames.QSBPlayerReady, true);
			DebugLog.DebugWrite("PlayerCameraSync init done - Request state!");
			QSBEventManager.FireEvent(EventNames.QSBPlayerStatesRequest);

			return body.gameObject;
		}

		protected override GameObject InitRemoteTransform()
		{
			var body = new GameObject("RemotePlayerCamera");

			PlayerToolsManager.Init(body.transform);

			var camera = body.AddComponent<Camera>();
			camera.enabled = false;
			var owcamera = body.AddComponent<OWCamera>();
			owcamera.fieldOfView = 70;
			owcamera.nearClipPlane = 0.1f;
			owcamera.farClipPlane = 50000f;
			Player.Camera = owcamera;
			Player.CameraBody = body;

			return body;
		}

		public override bool IsReady => Locator.GetPlayerTransform() != null
			&& Player != null
			&& QSBPlayerManager.PlayerExists(Player.PlayerId)
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U;
	}
}