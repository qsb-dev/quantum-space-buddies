using HarmonyLib;
using QSB.EchoesOfTheEye.DreamObjectProjectors.Messages;
using QSB.EchoesOfTheEye.DreamObjectProjectors.WorldObject;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamObjectProjectors.Patches;

internal class ProjectorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DreamObjectProjector), nameof(DreamObjectProjector.OnPressInteract))]
	public static void OnPressInteract(DreamObjectProjector __instance)
		=> __instance.GetWorldObject<QSBDreamObjectProjector>().SendMessage(new ProjectorStatusMessage(false));

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamObjectProjector), nameof(DreamObjectProjector.FixedUpdate))]
	public static bool FixedUpdate(DreamObjectProjector __instance)
	{
		var flag = __instance._lightSensor.IsIlluminated();
		if (!__instance._lit && flag && !__instance._wasSensorIlluminated)
		{
			__instance.SetLit(true);
			__instance.GetWorldObject<QSBDreamObjectProjector>().SendMessage(new ProjectorStatusMessage(true));
		}

		__instance._wasSensorIlluminated = flag;

		return false;
	}
}
