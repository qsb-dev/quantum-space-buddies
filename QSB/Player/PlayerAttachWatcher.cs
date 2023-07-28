using HarmonyLib;
using QSB.Patches;

namespace QSB.Player;

[HarmonyPatch(typeof(PlayerAttachPoint))]
public class PlayerAttachWatcher : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	public static PlayerAttachPoint Current { get; private set; }

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerAttachPoint.AttachPlayer))]
	private static void AttachPlayer(PlayerAttachPoint __instance)
	{
		if (Current != null)
		{
			Current.DetachPlayer();
		}

		Current = __instance;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerAttachPoint.DetachPlayer))]
	private static void DetachPlayer(PlayerAttachPoint __instance)
	{
		if (!__instance.enabled)
		{
			return;
		}

		Current = null;
	}
}