using QSB.Events;
using QSB.Patches;
using UnityEngine;

namespace QSB.QuantumSync
{
	public class ServerQuantumStateChangePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

		public override void DoPatches() => QSBCore.Helper.HarmonyHelper.AddPostfix<SocketedQuantumObject>("MoveToSocket", typeof(ServerQuantumStateChangePatches), nameof(Socketed_MoveToSocket));

		public static void Socketed_MoveToSocket(SocketedQuantumObject __instance, QuantumSocket socket)
		{
			var objId = QuantumManager.Instance.GetId(__instance);
			var socketId = QuantumManager.Instance.GetId(socket);
			GlobalMessenger<int, int, Quaternion>.FireEvent(EventNames.QSBSocketStateChange, objId, socketId, __instance.transform.localRotation);
		}
	}
}