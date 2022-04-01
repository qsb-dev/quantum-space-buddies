using HarmonyLib;
using QSB.EchoesOfTheEye.DreamObjectProjectors.WorldObject;
using QSB.EchoesOfTheEye.DreamRafts.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamRafts.Patches;

public class DreamRaftPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamRaftProjector), nameof(DreamRaftProjector.RespawnRaft))]
	private static void RespawnRaft(DreamRaftProjector __instance)
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
	}

	/// <summary>
	/// this is only called when:
	///	- you exit the dream world
	/// - the raft goes thru the warp volume with you not on it
	///
	/// we ignore both of these.
	/// we DO still suspend the raft so it's not visible.
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamRaftProjector), nameof(DreamRaftProjector.ExtinguishImmediately))]
	private static bool ExtinguishImmediately(DreamRaftProjector __instance)
	{
		if (!__instance._lit)
		{
			return false;
		}

		var projection = __instance._dreamRaftProjection;
		projection._body.Suspend();
		projection._waitingToSuspend = false;

		return false;
	}
}
