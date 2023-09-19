using HarmonyLib;
using QSB.EchoesOfTheEye.RaftSync.Messages;
using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Messaging;
using QSB.OwnershipSync;
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
		qsbRaft.NetworkBehaviour.netIdentity.UpdateOwnerQueue(OwnerQueueAction.Force);
		Delay.RunWhen(() => qsbRaft.NetworkBehaviour.isOwned, () =>
		{
			var normalized = Vector3.ProjectOnPlane(Locator.GetPlayerCamera().transform.forward, __instance.transform.up).normalized;
			__instance._raftBody.AddVelocityChange(normalized * 5f);
			__instance._effectsController.PlayRaftPush();
			__instance._pushTime = Time.time;
		});

		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RaftDock), nameof(RaftDock.OnPressInteract))]
	private static void RaftDock_OnPressInteract(RaftDock __instance)
	{
		if (Remote)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBRaftDock>().SendMessage(new RaftDockOnPressInteractMessage());
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(RaftCarrier), nameof(RaftCarrier.OnEntry))]
	private static bool RaftCarrier_OnEntry(RaftCarrier __instance, GameObject hitObj)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (hitObj.CompareTag("RaftDetector") && __instance._state == RaftCarrier.DockState.Ready)
		{
			var raft = hitObj.GetComponentInParent<RaftController>();
			var qsbRaft = raft.GetWorldObject<QSBRaft>();
			if (!qsbRaft.NetworkBehaviour.isOwned)
			{
				return false;
			}

			__instance.GetWorldObject<IQSBRaftCarrier>().SendMessage(new RaftCarrierOnEntryMessage(qsbRaft));
		}

		return true;
	}
}
