using HarmonyLib;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.RaftSync.Messages;
using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.Patches;

public class RaftPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RaftController), nameof(RaftController.OnPressInteract))]
	private static bool RaftController_OnPressInteract(RaftController __instance)
	{
		__instance._interactReceiver.SetInteractionEnabled(false);

		var qsbRaft = __instance.GetWorldObject<QSBRaft>();
		qsbRaft.TransformSync.netIdentity.UpdateAuthQueue(AuthQueueAction.Force);
		Delay.RunWhen(() => qsbRaft.TransformSync.hasAuthority, () =>
		{
			var normalized = Vector3.ProjectOnPlane(Locator.GetPlayerCamera().transform.forward, __instance.transform.up).normalized;
			__instance._raftBody.AddVelocityChange(normalized * 5f);
			__instance._effectsController.PlayRaftPush();
			__instance._pushTime = Time.time;
		});

		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RaftDock), nameof(RaftDock.OnEntry))]
	private static void RaftDock_OnEntry(RaftDock __instance)
	{
		if (__instance._state == RaftCarrier.DockState.AligningBelow)
		{
			__instance.GetWorldObject<QSBRaftDock>()
				.SendMessage(new DockRaftMessage(__instance._raft));
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RaftDock), nameof(RaftDock.OnPressInteract))]
	private static void RaftDock_OnPressInteract(RaftDock __instance) =>
		__instance.GetWorldObject<QSBRaftDock>().SendMessage(new UndockRaftMessage());

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DamRaftLift), nameof(DamRaftLift.OnEntry))]
	private static void DamRaftLift_OnEntry(DamRaftLift __instance)
	{
		if (__instance._state == RaftCarrier.DockState.AligningBelow)
		{
			new StartLiftingRaftMessage(__instance._raft.GetWorldObject<QSBRaft>()).Send();
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DamRaftLift), nameof(DamRaftLift.MoveRaftToNextNode))]
	private static void DamRaftLift_MoveRaftToNextNode(DamRaftLift __instance)
	{
		if (__instance._state == RaftCarrier.DockState.ResettingHook)
		{
			new StopLiftingRaftMessage().Send();
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DamRaftLift), nameof(DamRaftLift.OnDamBroken))]
	private static void DamRaftLift_OnDamBroken(DamRaftLift __instance)
	{
		if (__instance._raft != null)
		{
			new StopLiftingRaftMessage().Send();
		}
	}
}
