using QSB.Player;
using QSB.TransformSync;
using UnityEngine.Networking;

namespace QSB.Utility
{
	public static class UnetExtensions
	{
		public static PlayerInfo GetPlayer(this QSBNetworkConnection connection)
		{
			var go = connection.PlayerControllers[0].gameObject;
			var controller = go.GetComponent<PlayerTransformSync>();
			return QSBPlayerManager.GetPlayer(controller.NetId.Value);
		}
	}
}
