using HarmonyLib;
using QSB.EchoesOfTheEye.DreamObjectProjectors.Messages;
using QSB.EchoesOfTheEye.DreamObjectProjectors.WorldObject;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamObjectProjectors.Patches;

public class ProjectorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamObjectProjector), nameof(DreamObjectProjector.SetLit))]
	private static void SetLit(DreamObjectProjector __instance, bool lit)
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

		__instance.GetWorldObject<QSBDreamObjectProjector>()
			.SendMessage(new ProjectorLitMessage(lit));
	}
}
