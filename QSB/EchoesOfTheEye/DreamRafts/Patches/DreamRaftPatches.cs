using HarmonyLib;
using QSB.EchoesOfTheEye.DreamCandles.Patches;
using QSB.EchoesOfTheEye.DreamObjectProjectors.WorldObject;
using QSB.EchoesOfTheEye.DreamRafts.Messages;
using QSB.EchoesOfTheEye.DreamRafts.WorldObjects;
using QSB.Messaging;
using QSB.OwnershipSync;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamRafts.Patches;

public class DreamRaftPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamRaftProjector), nameof(DreamRaftProjector.RespawnRaft))]
	private static void RespawnRaft_Prefix(DreamRaftProjector __instance)
	{
		if (Remote)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamObjectProjector>()
			.SendMessage(new RespawnRaftMessage());

		// since respawning extinguishes all the candles, but we already have the above message
		DreamCandlePatches.DontSendMessage = true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DreamRaftProjector), nameof(DreamRaftProjector.RespawnRaft))]
	private static void RespawnRaft_Postfix(DreamRaftProjector __instance)
	{
		if (Remote)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		DreamCandlePatches.DontSendMessage = false;
	}

	/// <summary>
	/// this is only called when:
	///	- you exit the dream world
	/// - the raft goes thru the warp volume with you not on it
	///
	/// this is to suspend the raft so it doesn't fall endlessly.
	/// however, it's okay if it does that,
	/// and we don't want it to extinguish with other players on it.
	///
	/// BUG: this breaks when going thru the volume as a non auth player sometimes. oh well.
	/// TODO: use in-raft-volume trigger volume instead. just copy from the ringworld rafts. use this for fake sectors as well
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamWorldController), nameof(DreamWorldController.ExtinguishDreamRaft))]
	private static bool ExtinguishDreamRaft(DreamWorldController __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (__instance._lastUsedRaftProjector)
		{
			// still release authority over the raft tho
			__instance._lastUsedRaftProjector
				._dreamRaftProjection.GetComponent<DreamRaftController>().GetWorldObject<QSBDreamRaft>()
				.NetworkBehaviour.netIdentity.UpdateAuthQueue(OwnQueueAction.Remove);
		}

		return false;
	}
}
