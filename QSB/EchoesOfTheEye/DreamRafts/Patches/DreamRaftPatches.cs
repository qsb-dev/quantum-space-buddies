using HarmonyLib;
using QSB.EchoesOfTheEye.DreamRafts.Messages;
using QSB.EchoesOfTheEye.DreamRafts.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using System.Linq;

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

		if (__instance._lit == lit)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamRaftProjector>()
			.SendMessage(new SpawnRaftMessage());
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
			.SendMessage(new SpawnRaftMessage());
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamRaftProjection), nameof(DreamRaftProjection.OnCandleLitStateChanged))]
	private static bool OnCandleLitStateChanged(DreamRaftProjection __instance)
	{
		if (!__instance._visible)
		{
			return false;
		}

		if (__instance._candles.Any(x => x.IsLit()))
		{
			return false;
		}

		__instance.SetVisible(false);
		if (!Remote && QSBWorldSync.AllObjectsReady)
		{
			__instance.GetWorldObject<QSBDreamRaftProjection>()
				.SendMessage(new ExtinguishMessage());
		}

		return false;
	}
}
