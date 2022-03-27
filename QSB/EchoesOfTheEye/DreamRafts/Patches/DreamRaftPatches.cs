using HarmonyLib;
using QSB.EchoesOfTheEye.DreamRafts.Messages;
using QSB.EchoesOfTheEye.DreamRafts.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamRafts.Patches;

public class DreamRaftPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamRaftProjector), nameof(DreamRaftProjector.SetLit))]
	private static void SetLit(DreamRaftProjector __instance,
		bool lit)
	{
		if (Remote)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		if (__instance._lit == lit)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamRaftProjector>()
			.SendMessage(new SetLitMessage(lit));
	}

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

		__instance.GetWorldObject<QSBDreamRaftProjector>()
			.SendMessage(new RespawnRaftMessage());
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamRaftProjector), nameof(DreamRaftProjector.ExtinguishImmediately))]
	private static void ExtinguishImmediately(DreamRaftProjector __instance)
	{
		if (Remote)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		if (!__instance._lit)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamRaftProjector>()
			.SendMessage(new ExtinguishImmediatelyMessage());
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamRaftProjection), nameof(DreamRaftProjection.UpdateVisibility))]
	private static void UpdateVisibility(DreamRaftProjection __instance,
		bool immediate = false)
	{
		if (Remote)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamRaftProjection>()
			.SendMessage(new UpdateVisibilityMessage(__instance._visible, immediate));
	}
}
