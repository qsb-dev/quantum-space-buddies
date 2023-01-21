using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
using System.Linq;

namespace QSB.QuantumSync.Patches.Common;

[HarmonyPatch(typeof(QuantumShrine))]
public class QuantumShrinePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumShrine.IsPlayerInDarkness))]
	public static bool IsPlayerInDarkness(QuantumShrine __instance, out bool __result)
	{
		foreach (var lamp in __instance._lamps)
		{
			if (lamp.intensity > 0f)
			{
				__result = false;
				return false;
			}
		}

		var playersInMoon = QSBPlayerManager.PlayerList.Where(x => x.IsInMoon).ToList();

		if (playersInMoon.Any(player => !player.IsInShrine))
		{
			__result = false;
			return false;
		}

		if (playersInMoon.Any(player => player.FlashLight != null && player.FlashLight.FlashlightOn))
		{
			__result = false;
			return false;
		}

		if (playersInMoon.Count == 0)
		{
			__result = false;
			return false;
		}

		if (QSBPlayerManager.LocalPlayer != null
			&& QSBPlayerManager.LocalPlayer.IsInShrine
			&& PlayerState.IsFlashlightOn())
		{
			__result = false;
			return false;
		}

		// BUG : make this *really* check for all players - check other probes and other jetpacks!
		__result = __instance._gate.GetOpenFraction() == 0f
				   && !__instance._isProbeInside
				   && Locator.GetThrusterLightTracker().GetLightRange() <= 0f;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumShrine.ChangeQuantumState))]
	public static bool ChangeQuantumState(QuantumShrine __instance)
	{
		var shrineWorldObject = __instance.GetWorldObject<QSBSocketedQuantumObject>();
		var isInControl = shrineWorldObject.ControllingPlayer == QSBPlayerManager.LocalPlayerId;
		return isInControl;
	}
}
