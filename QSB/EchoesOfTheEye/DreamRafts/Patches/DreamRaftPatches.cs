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
	/// make the projectors invisible,
	/// but don't change the lit state
	/// or sync anything.
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamRaftProjector), nameof(DreamRaftProjector.ExtinguishImmediately))]
	private static bool ExtinguishImmediately(DreamRaftProjector __instance)
	{
		if (!__instance._lit)
		{
			return false;
		}

		foreach (var projection in __instance._projections)
		{
			projection.SetVisibleImmediate(false, true);
		}

		return false;
	}
}
