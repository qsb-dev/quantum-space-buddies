using OWML.Utils;
using QSB.Patches;

namespace QSB.TimeSync.Patches
{
	public class WakeUpPatches : QSBPatch
	{
		public override PatchType Type => PatchType.OnNonServerClientConnect;

		public static bool OnStartOfTimeLoopPrefix(ref PlayerCameraEffectController __instance)
		{
			if (__instance.gameObject.CompareTag("MainCamera") && QSBSceneManager.CurrentScene != OWScene.EyeOfTheUniverse)
			{
				__instance.Invoke("WakeUp");
			}
			return false;
		}

		public override void DoPatches() => QSBCore.Helper.HarmonyHelper.AddPrefix<PlayerCameraEffectController>("OnStartOfTimeLoop", typeof(WakeUpPatches), nameof(OnStartOfTimeLoopPrefix));

		public override void DoUnpatches() => QSBCore.Helper.HarmonyHelper.Unpatch<PlayerCameraEffectController>("OnStartOfTimeLoop");
	}
}