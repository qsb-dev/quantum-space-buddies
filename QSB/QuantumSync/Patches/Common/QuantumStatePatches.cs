using HarmonyLib;
using OWML.Common;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;

namespace QSB.QuantumSync.Patches.Common;

[HarmonyPatch(typeof(QuantumState))]
public class QuantumStatePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(QuantumState.SetVisible))]
	public static void SetVisible(QuantumState __instance, bool visible)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		if (!visible)
		{
			return;
		}

		var allMultiStates = QSBWorldSync.GetWorldObjects<QSBMultiStateQuantumObject>();
		var stateObject = __instance.GetWorldObject<QSBQuantumState>();
		var owner = allMultiStates.FirstOrDefault(x => x.QuantumStates.Contains(stateObject));
		if (owner == default)
		{
			DebugLog.ToConsole($"Error - Could not find QSBMultiStateQuantumObject for state {__instance.name}", MessageType.Error);
			return;
		}

		if (owner.ControllingPlayer != QSBPlayerManager.LocalPlayerId)
		{
			return;
		}

		var stateIndex = owner.QuantumStates.IndexOf(stateObject);
		owner.SendMessage(new MultiStateChangeMessage(stateIndex));
	}
}
