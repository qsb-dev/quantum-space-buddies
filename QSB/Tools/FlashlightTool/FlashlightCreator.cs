using QSB.Player;
using UnityEngine;

namespace QSB.Tools.FlashlightTool
{
	internal static class FlashlightCreator
	{
		internal static void CreateFlashlight(PlayerInfo player)
		{
			var flashlightRoot = player.CameraBody.transform.Find("REMOTE_FlashlightRoot");

			var component = flashlightRoot.GetComponent<QSBFlashlight>();
			component.Player = player;
			component.Init();
		}
	}
}
