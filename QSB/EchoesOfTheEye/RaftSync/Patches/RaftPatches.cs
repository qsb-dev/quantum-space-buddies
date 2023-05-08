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
		qsbRaft.NetworkBehaviour.netIdentity.UpdateAuthQueue(AuthQueueAction.Force);
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

		__instance.GetWorldObject<QSBRaftDock>().SendMessage(new RaftDockOnPressInteractMessage());
	}
}
