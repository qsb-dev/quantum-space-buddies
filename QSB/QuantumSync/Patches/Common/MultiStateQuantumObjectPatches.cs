using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.QuantumSync.Patches.Common;

[HarmonyPatch(typeof(MultiStateQuantumObject))]
public class MultiStateQuantumObjectPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(MultiStateQuantumObject.Start))]
	public static bool Start(MultiStateQuantumObject __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbObj = __instance.GetWorldObject<QSBMultiStateQuantumObject>();
		if (qsbObj.ControllingPlayer == 0)
		{
			return true;
		}

		foreach (var state in qsbObj.QuantumStates)
		{
			if (!state.IsMeantToBeEnabled)
			{
				state.SetVisible(false);
			}
		}

		if (__instance._sector == null)
		{
			__instance.CheckEnabled();
		}

		if (__instance._collapseOnStart)
		{
			__instance.Collapse(true);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(MultiStateQuantumObject.ChangeQuantumState))]
	public static bool ChangeQuantumState(MultiStateQuantumObject __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbObj = __instance.GetWorldObject<QSBMultiStateQuantumObject>();
		if (qsbObj.ControllingPlayer == 0 && qsbObj.CurrentState == -1)
		{
			return true;
		}

		var isInControl = qsbObj.ControllingPlayer == QSBPlayerManager.LocalPlayerId;
		return isInControl;
	}
}
