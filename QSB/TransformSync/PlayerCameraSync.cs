using QSB.Events;
using QSB.Player;
using QSB.Tools;
using QSB.Utility;
using UnityEngine;

namespace QSB.TransformSync
{
	public class PlayerCameraSync : TransformSync
	{
		protected override Transform InitLocalTransform()
		{
			var body = Locator.GetPlayerCamera().gameObject.transform;

			Player.Camera = Locator.GetPlayerCamera();
			Player.CameraBody = body.gameObject;

			Player.IsReady = true;
			EventManager.FireEvent(EventNames.QSBPlayerReady, true);
			DebugLog.DebugWrite("PlayerCameraSync init done - Request state!");
			EventManager.FireEvent(EventNames.QSBPlayerStatesRequest);

			return body;
		}

		protected override Transform InitRemoteTransform()
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

			return body.transform;
		}

		private void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp || !Player.IsReady || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug)
			{
				return;
			}
			Popcron.Gizmos.Frustum(Player.Camera);
		}

		public override bool IsReady => Locator.GetPlayerTransform() != null
			&& Player != null
			&& PlayerManager.PlayerExists(Player.PlayerId)
			&& NetId.Value != uint.MaxValue
			&& NetId.Value != 0U;
	}
}