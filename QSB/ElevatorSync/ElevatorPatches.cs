using OWML.ModHelper.Events;
using QSB.Events;
using QSB.Utility;

namespace QSB.ElevatorSync
{
    public static class ElevatorPatches
    {
        public static void StartLift(Elevator __instance)
        {
            var isGoingUp = __instance.GetValue<bool>("_goingToTheEnd");
            var direction = isGoingUp ? ElevatorDirection.Up : ElevatorDirection.Down;
            DebugLog.ToAll($"StartLift. Direction: {direction}");
            GlobalMessenger<ElevatorDirection>.FireEvent(EventNames.QSBStartLift, direction);
        }
    }
}
