using OWML.Utils;
using QSB.Events;
using QSB.Patches;

namespace QSB.ElevatorSync.Patches
{
	public class ElevatorPatches : QSBPatch
	{
		public override PatchType Type => PatchType.OnModStart;

		public static void StartLift(Elevator __instance)
		{
			var isGoingUp = __instance.GetValue<bool>("_goingToTheEnd");
			var id = ElevatorManager.Instance.GetId(__instance);
			EventManager.FireEvent(EventNames.QSBStartLift, id, isGoingUp);
		}

		public override void DoPatches() => QSBCore.Helper.HarmonyHelper.AddPostfix<Elevator>("StartLift", typeof(ElevatorPatches), nameof(StartLift));

		public override void DoUnpatches() => QSBCore.Helper.HarmonyHelper.Unpatch<Elevator>("StartLift");
	}
}