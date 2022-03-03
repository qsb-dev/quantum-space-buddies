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
	private static bool OnPressInteract(RaftController __instance)
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

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RaftDock), nameof(RaftDock.OnPressInteract))]
	private static bool OnPressInteract(RaftDock __instance)
	{
		if (__instance._raft != null && __instance._state == RaftCarrier.DockState.Docked)
		{
			__instance._raftUndockCountDown = __instance._raft.dropDelay;
			__instance._state = RaftCarrier.DockState.WaitForExit;
			__instance._raft.SetRailingRaised(true);
			if (__instance._gearInterface != null)
			{
				__instance._gearInterface.AddRotation(90f);
			}

			__instance.enabled = true;

			__instance.GetWorldObject<QSBRaftDock>().SendMessage(new RaftDockOnPressInteractMessage());
			return false;
		}

		if (__instance._gearInterface != null)
		{
			__instance._gearInterface.PlayFailure();
		}

		return false;
	}
}