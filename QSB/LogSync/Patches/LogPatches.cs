using QSB.Events;
using QSB.Patches;

namespace QSB.LogSync.Patches
{
	public class LogPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public static void ShipLogManager_RevealFact(string id, bool saveGame, bool showNotification, bool __result)
		{
			if (!__result)
			{
				return;
			}

			QSBEventManager.FireEvent(EventNames.QSBRevealFact, id, saveGame, showNotification);
		}

		public override void DoPatches() => Postfix(nameof(ShipLogManager_RevealFact));
	}
}