using QSB.EchoesOfTheEye.LightSensorSync;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.Messages;
using QSB.SectorSync;
using QSB.Tools;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.PlayerBodySetup.Local;

public static class LocalPlayerCreation
{
	public static Transform CreatePlayer(
		PlayerInfo player,
		QSBSectorDetector sectorDetector,
		out Transform visibleCameraRoot,
		out Transform visibleRoastingSystem,
		out Transform visibleStickPivot,
		out Transform visibleStickTip)
	{
		sectorDetector.Init(Locator.GetPlayerSectorDetector());

		// player body
		var playerBody = Locator.GetPlayerTransform();
		var playerModel = playerBody.Find("Traveller_HEA_Player_v2");
		player.AnimationSync.InitLocal(playerModel);
		player.Body = playerBody.gameObject;
		player.ThrusterLightTracker = player.Body.GetComponentInChildren<ThrusterLightTracker>();

		// camera
		var cameraBody = Locator.GetPlayerCamera().gameObject.transform;
		player.Camera = Locator.GetPlayerCamera();
		player.CameraBody = cameraBody.gameObject;
		visibleCameraRoot = cameraBody;

		PlayerToolsManager.InitLocal();

		// stick
		var pivot = QSBWorldSync.GetUnityObject<RoastingStickController>().transform.Find("Stick_Root/Stick_Pivot");
		player.RoastingStick = pivot.parent.gameObject;
		visibleRoastingSystem = pivot.parent.parent;
		visibleStickPivot = pivot;
		visibleStickTip = pivot.Find("Stick_Tip");

		player.IsReady = true;
		new PlayerReadyMessage(true).Send();

		new RequestStateResyncMessage().Send();

		return playerBody;
	}
}