using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Mirror;
using NewHorizons;
using QSB;
using QSB.Patches;
using QSB.Player;
using QSB.SaveSync.Messages;
using QSB.Utility;

namespace QSBNH.Patches;


internal class GameStateMessagePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	private static string _initialSystem;
	private static int[] _hostAddonHash;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameStateMessage), nameof(GameStateMessage.Serialize))]
	public static void GameStateMessage_Serialize(GameStateMessage __instance, NetworkWriter writer)
	{
		var currentSystem = QSBNH.Instance.NewHorizonsAPI.GetCurrentStarSystem();

		writer.Write(currentSystem);
		writer.WriteArray(QSBNH.HashAddonsForSystem(currentSystem));
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameStateMessage), nameof(GameStateMessage.Deserialize))]
	public static void GameStateMessage_Deserialize(GameStateMessage __instance, NetworkReader reader)
	{
		_initialSystem = reader.Read<string>();
		_hostAddonHash = reader.ReadArray<int>();
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(GameStateMessage), nameof(GameStateMessage.OnReceiveRemote))]
	public static void GameStateMessage_OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.DebugWrite($"Why is the host being given the initial state info?");
		}
		else
		{
			DebugLog.DebugWrite($"Player#{QSBPlayerManager.LocalPlayerId} is being sent to {_initialSystem}");

			WarpManager.RemoteChangeStarSystem(_initialSystem, false, false, _hostAddonHash);
		}
	}
}
