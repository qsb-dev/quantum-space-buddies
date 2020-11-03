using OWML.ModHelper.Events;
using QSB.Events;

namespace QSB.ElevatorSync
{
    public class ElevatorPatches : QSBPatch
    {
        public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

        public static void StartLift(Elevator __instance)
        {
            var isGoingUp = __instance.GetValue<bool>("_goingToTheEnd");
            var direction = isGoingUp ? ElevatorDirection.Up : ElevatorDirection.Down;
            var id = ElevatorManager.Instance.GetId(__instance);
            GlobalMessenger<int, ElevatorDirection>.FireEvent(EventNames.QSBStartLift, id, direction);
        }

        public override void DoPatches()
        {
            QSB.Helper.HarmonyHelper.AddPostfix<Elevator>("StartLift", typeof(ElevatorPatches), nameof(StartLift));
        }
    }
}
