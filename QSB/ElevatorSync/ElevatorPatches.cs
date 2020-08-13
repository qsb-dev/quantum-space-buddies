using OWML.ModHelper.Events;
using QSB.Events;

namespace QSB.ElevatorSync
{
    public static class ElevatorPatches
    {
        public static void StartLift(Elevator __instance)
        {
            var isGoingUp = __instance.GetValue<bool>("_goingToTheEnd");
            var direction = isGoingUp ? ElevatorDirection.Up : ElevatorDirection.Down;
            var id = ElevatorManager.Instance.GetId(__instance);
            GlobalMessenger<int, ElevatorDirection>.FireEvent(EventNames.QSBStartLift, id, direction);
        }
    }
}
