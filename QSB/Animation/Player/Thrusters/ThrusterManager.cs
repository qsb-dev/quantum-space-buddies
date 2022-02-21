using QSB.Player;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters
{
	internal static class ThrusterManager
	{
		public static void CreateRemotePlayerVFX(PlayerInfo player)
		{
			var newVfx = player.Body.transform.Find("REMOTE_PlayerVFX").gameObject;

			CreateThrusterWashController(newVfx.transform.Find("ThrusterWash").gameObject, player);
			CreateThrusterFlameController(newVfx, player);
		}

		private static void CreateThrusterFlameController(GameObject root, PlayerInfo player)
		{
			var existingControllers = root.GetComponentsInChildren<RemoteThrusterFlameController>(true);
			foreach (var controller in existingControllers)
			{
				controller.Init(player);
			}
		}

		private static void CreateThrusterWashController(GameObject root, PlayerInfo player)
		{
			var newObj = root.GetComponent<RemoteThrusterWashController>();
			newObj.Init(player);
		}
	}
}
