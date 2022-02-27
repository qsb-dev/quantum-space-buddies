using HarmonyLib;
using QSB.LogSync.Messages;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.LogSync.Patches
{
	[HarmonyPatch]
	public class LogPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.RevealFact))]
		public static void ShipLogManager_RevealFact(string id, bool saveGame, bool showNotification, bool __result)
		{
			if (!__result)
			{
				return;
			}

			new RevealFactMessage(id, saveGame, showNotification).Send();
		}
	}
}