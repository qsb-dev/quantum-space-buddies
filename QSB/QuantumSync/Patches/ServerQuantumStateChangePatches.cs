using QSB.Events;
using QSB.Patches;
using QSB.WorldSync;
using System;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Patches
{
	public class ServerQuantumStateChangePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPostfix<SocketedQuantumObject>("MoveToSocket", typeof(ServerQuantumStateChangePatches), nameof(Socketed_MoveToSocket));
			QSBCore.Helper.HarmonyHelper.AddPostfix<QuantumState>("SetVisible", typeof(ServerQuantumStateChangePatches), nameof(QuantumState_SetVisible));
		}

		public static void Socketed_MoveToSocket(SocketedQuantumObject __instance, QuantumSocket socket)
		{
			var objId = QuantumManager.Instance.GetId(__instance);
			var socketId = QuantumManager.Instance.GetId(socket);
			GlobalMessenger<int, int, Quaternion>
				.FireEvent(EventNames.QSBSocketStateChange,
					objId,
					socketId,
					__instance.transform.localRotation);
		}

		public static void QuantumState_SetVisible(QuantumState __instance, bool visible)
		{
			var allMultiStates = QSBWorldSync.GetWorldObjects<QSBMultiStateQuantumObject>();
			var owner = allMultiStates.First(x => x.QuantumStates.Contains(__instance));
			GlobalMessenger<int, int>
				.FireEvent(EventNames.QSBMultiStateChange,
					QuantumManager.Instance.GetId(owner.AttachedObject),
					Array.IndexOf(owner.QuantumStates, __instance));
		}
	}
}