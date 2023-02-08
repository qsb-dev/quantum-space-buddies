using HarmonyLib;
using OWML.Common;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Patches.Common;

[HarmonyPatch(typeof(SocketedQuantumObject))]
public class SocketedQuantumObjectPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SocketedQuantumObject.ChangeQuantumState))]
	public static bool ChangeQuantumState(
		SocketedQuantumObject __instance,
		ref bool __result,
		bool skipInstantVisibilityCheck)
	{
		if (QSBWorldSync.AllObjectsReady)
		{
			var socketedWorldObject = __instance.GetWorldObject<QSBSocketedQuantumObject>();
			if (socketedWorldObject.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
			{
				return false;
			}
		}

		foreach (var socket in __instance._childSockets)
		{
			if (socket.IsOccupied())
			{
				__result = false;
				return false;
			}
		}

		if (__instance._socketList.Count <= 1)
		{
			DebugLog.ToConsole($"Error - Not enough quantum sockets in list for {__instance.name}!", MessageType.Error);
			__result = false;
			return false;
		}

		var list = new List<QuantumSocket>();
		foreach (var socket in __instance._socketList)
		{
			if (!socket.IsOccupied() && socket.IsActive())
			{
				list.Add(socket);
			}
		}

		if (list.Count == 0)
		{
			__result = false;
			return false;
		}

		if (__instance._recentlyObscuredSocket != null)
		{
			__instance.MoveToSocket(__instance._recentlyObscuredSocket);
			__instance._recentlyObscuredSocket = null;
			__result = true;
			return false;
		}

		var occupiedSocket = __instance._occupiedSocket;
		for (var i = 0; i < 20; i++)
		{
			var index = Random.Range(0, list.Count);
			__instance.MoveToSocket(list[index]);
			if (skipInstantVisibilityCheck)
			{
				__result = true;
				return false;
			}

			bool socketNotSuitable;
			var isSocketIlluminated = __instance.CheckIllumination();

			var playersEntangled = QuantumManager.GetEntangledPlayers(__instance);
			if (playersEntangled.Count() != 0)
			{
				// socket not suitable if illuminated
				socketNotSuitable = isSocketIlluminated;
			}
			else
			{
				var checkVisInstant = __instance.CheckVisibilityInstantly();
				if (isSocketIlluminated)
				{
					// socket not suitable if object is visible
					socketNotSuitable = checkVisInstant;
				}
				else
				{
					// socket not suitable if player is inside object
					socketNotSuitable = playersEntangled.Any(x => __instance.CheckPointInside(x.CameraBody.transform.position));
				}
			}

			if (!socketNotSuitable)
			{
				__result = true;
				return false;
			}

			list.RemoveAt(index);
			if (list.Count == 0)
			{
				break;
			}
		}

		__instance.MoveToSocket(occupiedSocket);
		__result = false;
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(SocketedQuantumObject.MoveToSocket))]
	public static void MoveToSocket(SocketedQuantumObject __instance, QuantumSocket socket)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		if (socket == null)
		{
			DebugLog.ToConsole($"Error - Trying to move {__instance.name} to a null socket!", MessageType.Error);
			return;
		}

		var objectWorldObject = __instance.GetWorldObject<QSBSocketedQuantumObject>();
		var socketWorldObject = socket.GetWorldObject<QSBQuantumSocket>();

		if (objectWorldObject == null)
		{
			DebugLog.ToConsole($"Worldobject is null for {__instance.name}!");
			return;
		}

		if (objectWorldObject.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
		{
			return;
		}

		objectWorldObject.SendMessage(new SocketStateChangeMessage(
			socketWorldObject.ObjectId,
			__instance.transform.localRotation));
	}
}
