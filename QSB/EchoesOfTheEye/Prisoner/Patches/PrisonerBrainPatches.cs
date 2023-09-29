using HarmonyLib;
using QSB.EchoesOfTheEye.Prisoner.Messages;
using QSB.EchoesOfTheEye.Prisoner.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using System;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner.Patches;

[HarmonyPatch(typeof(PrisonerBrain))]
public class PrisonerBrainPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonerBrain.Start))]
	public static bool Start(PrisonerBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBPrisonerBrain>().Start();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonerBrain.BeginBehavior), new Type[] { typeof(PrisonerBehavior), typeof(Transform), typeof(float) })]
	public static bool BeginBehaviour(PrisonerBrain __instance, PrisonerBehavior behavior, Transform marker, float delay)
	{
		if (!QSBCore.IsHost)
		{
			return false;
		}

		if (delay > 0f)
		{
			__instance._pendingBehavior = behavior;
			__instance._pendingBehaviorCueMarker = marker;
			__instance._pendingBehaviorEntryTime = Time.time + delay;
			return false;
		}

		__instance.ExitBehavior(__instance._currentBehavior);
		var currentBehavior = __instance._currentBehavior;
		__instance._currentBehavior = behavior;
		__instance._behaviorCueMarker = marker;
		__instance._pendingBehavior = PrisonerBehavior.None;
		__instance._pendingBehaviorCueMarker = null;
		__instance.EnterBehavior(__instance._currentBehavior, currentBehavior);

		__instance.GetWorldObject<QSBPrisonerBrain>().SendMessage(new PrisonerEnterBehaviourMessage(behavior, marker?.GetComponent<PrisonerBehaviourCueMarker>()));

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonerBrain.Update))]
	public static bool Update(PrisonerBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBPrisonerBrain>().Update();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonerBrain.FixedUpdate))]
	public static bool FixedUpdate(PrisonerBrain __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		__instance.GetWorldObject<QSBPrisonerBrain>().FixedUpdate();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonerBrain.OnFinishEmergeAnimation))]
	public static bool OnFinishEmergeAnimation(PrisonerBrain __instance)
	{
		if (__instance._currentBehavior is PrisonerBehavior.Emerge or PrisonerBehavior.WaitForConversation)
		{
			__instance._effects.OnRevealAnimationComplete -= __instance.OnFinishEmergeAnimation;
			__instance.OnFinishEmergeBehavior.Invoke();
		}

		return false;
	}
}