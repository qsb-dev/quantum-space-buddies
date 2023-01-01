using GhostEnums;
using HarmonyLib;
using QSB.EchoesOfTheEye.Ghosts.Messages;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using System.Reflection;

namespace QSB.EchoesOfTheEye.Ghosts.Patches;

[HarmonyPatch(typeof(GhostController))]
internal class GhostControllerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostController.Initialize))]
	public static bool Initialize(GhostController __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostController.SetLanternConcealed))]
	public static bool SetLanternConcealed(GhostController __instance, bool concealed, bool playAudio)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;

		}
		__instance.GetWorldObject<QSBGhostController>().SetLanternConcealed(concealed, playAudio);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostController.ChangeLanternFocus))]
	public static bool ChangeLanternFocus(GhostController __instance, float focus, float focusRate)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostController>().ChangeLanternFocus(focus, focusRate);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostController.FacePlayer))]
	public static bool FacePlayer(GhostController __instance, TurnSpeed turnSpeed)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostController.SetNodeMap))]
	public static bool SetNodeMap(GhostController __instance, GhostNodeMap nodeMap)
	{
		if (__instance._nodeMap == nodeMap)
		{
			return false;
		}

		__instance.StopMoving();
		__instance.StopFacing();
		__instance._nodeMap = nodeMap;
		__instance.transform.parent = nodeMap.transform;
		__instance._nodeRoot = nodeMap.transform;
		__instance.OnNodeMapChanged.Invoke();

		__instance.GetWorldObject<QSBGhostController>().SendMessage(new ChangeNodeMapMessage(nodeMap.GetWorldObject<QSBGhostNodeMap>().ObjectId));

		return false;
	}
}
