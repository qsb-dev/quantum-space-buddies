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
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Patches;

[HarmonyPatch(typeof(GhostBrain))]
public class GhostBrainPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.GetCurrentActionName))]
	public static bool GetCurrentActionName(GhostBrain __instance, ref GhostAction.Name __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__result = __instance.GetWorldObject<QSBGhostBrain>().GetCurrentActionName();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.GetCurrentAction))]
	public static bool GetCurrentAction(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.GetAction))]
	public static bool GetAction(GhostBrain __instance, GhostAction.Name actionName)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.GetThreatAwareness))]
	public static bool GetThreatAwareness(GhostBrain __instance, ref GhostData.ThreatAwareness __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__result = __instance.GetWorldObject<QSBGhostBrain>().GetThreatAwareness();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.GetEffects))]
	public static bool GetEffects(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.CheckDreadAudioConditions))]
	public static bool CheckDreadAudioConditions(GhostBrain __instance, ref bool __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__result = __instance.GetWorldObject<QSBGhostBrain>().CheckDreadAudioConditions();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.CheckFearAudioConditions))]
	public static bool CheckFearAudioConditions(GhostBrain __instance, bool fearAudioAlreadyPlaying, ref bool __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__result = __instance.GetWorldObject<QSBGhostBrain>().CheckFearAudioConditions(fearAudioAlreadyPlaying);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.Awake))]
	public static bool Awake(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().Awake();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.Start))]
	public static bool Start(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().Start();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.OnDestroy))]
	public static bool OnDestroy(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().OnDestroy();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.TabulaRasa))]
	public static bool TabulaRasa(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().TabulaRasa();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.Die))]
	public static bool Die(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().Die();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.EscalateThreatAwareness))]
	public static bool EscalateThreatAwareness(GhostBrain __instance, GhostData.ThreatAwareness newThreatAwareness)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().EscalateThreatAwareness(newThreatAwareness);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.WakeUp))]
	public static bool WakeUp(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().WakeUp();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.HearGhostCall))]
	public static bool HearGhostCall(GhostBrain __instance, Vector3 playerLocalPosition, float reactDelay, bool playResponseAudio, ref bool __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__result = __instance.GetWorldObject<QSBGhostBrain>().HearGhostCall(playerLocalPosition, reactDelay, playResponseAudio);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.HearCallForHelp))]
	public static bool HearCallForHelp(GhostBrain __instance, Vector3 playerLocalPosition, float reactDelay, ref bool __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.HintPlayerLocation), new Type[] { })]
	public static bool HintPlayerLocation(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.HintPlayerLocation), typeof(Vector3), typeof(float))]
	public static bool HintPlayerLocation(GhostBrain __instance, Vector3 localPosition, float informationTime)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.FixedUpdate))]
	public static bool FixedUpdate(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().FixedUpdate();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.Update))]
	public static bool Update(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().Update();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.FixedUpdate_ThreatAwareness))]
	public static bool FixedUpdate_ThreatAwareness(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().FixedUpdate_ThreatAwareness();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.EvaluateActions))]
	public static bool EvaluateActions(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().EvaluateActions();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.ChangeAction), typeof(GhostAction.Name))]
	public static bool ChangeAction(GhostBrain __instance, GhostAction.Name actionName)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.ChangeAction), typeof(GhostAction))]
	public static bool ChangeAction(GhostBrain __instance, GhostAction action)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.ClearPendingAction))]
	public static bool ClearPendingAction(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().ClearPendingAction();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.OnArriveAtPosition))]
	public static bool OnArriveAtPosition(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().OnArriveAtPosition();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.OnTraversePathNode))]
	public static bool OnTraversePathNode(GhostBrain __instance, GhostNode node)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().OnTraversePathNode(node);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.OnFaceNode))]
	public static bool OnFaceNode(GhostBrain __instance, GhostNode node)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().OnFaceNode(node);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.OnFinishFaceNodeList))]
	public static bool OnFinishFaceNodeList(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().OnFinishFaceNodeList();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.OnCallForHelp))]
	public static bool OnCallForHelp(GhostBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBGhostBrain>().OnCallForHelp();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.OnEnterDreamWorld))]
	public static bool OnEnterDreamWorld(GhostBrain __instance)
	{
		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostBrain.OnExitDreamWorld))]
	public static bool OnExitDreamWorld(GhostBrain __instance)
	{
		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}
}
