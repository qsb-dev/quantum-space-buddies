using QSB.Events;
using QSB.Patches;
using QSB.Utility;
using System.Linq;

namespace QSB.DeathSync.Patches
{
	public class DeathPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

		public static bool PreFinishDeathSequence(DeathType deathType)
		{
			if (RespawnOnDeath.Instance == null)
			{
				return true;
			}

			if (RespawnOnDeath.Instance.AllowedDeathTypes.Contains(deathType))
			{
				DebugLog.DebugWrite($"Allowing death of {deathType}");
				return true;
			}

			DebugLog.DebugWrite($"Not allowing death of {deathType}");

			RespawnOnDeath.Instance.ResetShip();
			RespawnOnDeath.Instance.ResetPlayer();
			return false;
		}

		public static void BroadcastDeath(DeathType deathType) => GlobalMessenger<DeathType>.FireEvent(EventNames.QSBPlayerDeath, deathType);

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(DeathPatches), nameof(PreFinishDeathSequence));
			QSBCore.Helper.HarmonyHelper.AddPostfix<DeathManager>("KillPlayer", typeof(DeathPatches), nameof(BroadcastDeath));
		}
	}
}