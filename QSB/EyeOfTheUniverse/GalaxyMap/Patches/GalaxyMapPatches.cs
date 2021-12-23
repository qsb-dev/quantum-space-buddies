using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System.Linq;

namespace QSB.EyeOfTheUniverse.GalaxyMap.Patches
{
	internal class GalaxyMapPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(GalaxyMapController), nameof(GalaxyMapController.OnPressInteract))]
		public static bool OnPressInteractPrefix()
		{
			var allInObservatory = QSBPlayerManager.PlayerList.All(x => x.EyeState == EyeState.Observatory);
			DebugLog.DebugWrite($"Prefix - all players in observatory = {allInObservatory}");

			if (!allInObservatory)
			{
				GalaxyMapManager.Instance.Tree.StartConversation();
			}

			return allInObservatory;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GalaxyMapController), nameof(GalaxyMapController.OnPressInteract))]
		public static void OnPressInteractPostfix()
		{
			if (QSBPlayerManager.PlayerList.All(x => x.EyeState == EyeState.Observatory))
			{
				QSBEventManager.FireEvent(EventNames.QSBZoomOut);
			}
		}
	}
}
