using QSB.EventsCore;
using System.Linq;

namespace QSB.DeathSync
{
    public class DeathPatches : QSBPatch
    {
        public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

        public static bool PreFinishDeathSequence(DeathType deathType)
        {
            if (RespawnOnDeath.Instance.AllowedDeathTypes.Contains(deathType))
            {
                // Allow real death
                return true;
            }

            RespawnOnDeath.Instance.ResetShip();
            RespawnOnDeath.Instance.ResetPlayer();

            // Prevent original death method from running.
            return false;
        }

        public static void BroadcastDeath(DeathType deathType)
        {
            GlobalMessenger<DeathType>.FireEvent(EventNames.QSBPlayerDeath, deathType);
        }

        public override void DoPatches()
        {
            QSB.Helper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(DeathPatches), nameof(PreFinishDeathSequence));
            QSB.Helper.HarmonyHelper.AddPostfix<DeathManager>("KillPlayer", typeof(DeathPatches), nameof(BroadcastDeath));
        }
    }
}