using HarmonyLib;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Patches;

[HarmonyPatch(typeof(GhostEffects))]
internal class GhostEffectsPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostEffects.Initialize))]
	public static bool Initialize(GhostEffects __instance, Transform nodeRoot, GhostController controller, GhostData data)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostEffects.AllowFootstepAudio))]
	public static bool AllowFootstepAudio(GhostEffects __instance, bool usingTimer, ref bool __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__result = __instance.GetWorldObject<QSBGhostEffects>().AllowFootstepAudio(usingTimer);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostEffects.PlayLanternAudio))]
	public static bool PlayLanternAudio(GhostEffects __instance, AudioType audioType)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostEffects>().PlayLanternAudio(audioType);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostEffects.Update_Effects))]
	public static bool Update_Effects(GhostEffects __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostEffects>().Update_Effects();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostEffects.OnSectorOccupantsUpdated))]
	public static bool OnSectorOccupantsUpdated(GhostEffects __instance)
	{
		__instance._animator.enabled = __instance._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe)
			|| QSBPlayerManager.PlayerList.Any(x => x.InDreamWorld);

		return false;
	}
}
