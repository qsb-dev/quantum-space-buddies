using GhostEnums;
using HarmonyLib;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Ghosts.Patches;

[HarmonyPatch(typeof(GhostController))]
internal class GhostControllerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostController.Initialize))]
	public static bool Initialize(GhostController __instance)
	{
		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostController.SetLanternConcealed))]
	public static bool SetLanternConcealed(GhostController __instance, bool concealed, bool playAudio)
	{
		__instance.GetWorldObject<QSBGhostController>().SetLanternConcealed(concealed, playAudio);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostController.ChangeLanternFocus))]
	public static bool ChangeLanternFocus(GhostController __instance, float focus, float focusRate)
	{
		__instance.GetWorldObject<QSBGhostController>().ChangeLanternFocus(focus, focusRate);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostController.FacePlayer))]
	public static bool FacePlayer(GhostController __instance, TurnSpeed turnSpeed)
	{
		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}
}
