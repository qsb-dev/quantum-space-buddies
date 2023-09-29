using QSB.Player;

namespace QSB.Tools.FlashlightTool;

public static class FlashlightCreator
{
	internal static void CreateFlashlight(PlayerInfo player)
	{
		var REMOTE_FlashlightRoot = player.CameraBody.transform.Find("REMOTE_FlashlightRoot").gameObject;

		var qsbFlashlight = REMOTE_FlashlightRoot.GetComponent<QSBFlashlight>();
		qsbFlashlight.Player = player;
		qsbFlashlight.Init();
	}
}