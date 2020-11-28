using OWML.ModHelper.Events;

namespace QSB.TimeSync
{
	public class WakeUpPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		public static bool OnStartOfTimeLoopPrefix(ref PlayerCameraEffectController __instance)
		{
			if (__instance.gameObject.CompareTag("MainCamera") && QSBSceneManager.CurrentScene != OWScene.EyeOfTheUniverse)
			{
				__instance.Invoke("WakeUp");
			}
			return false;
		}

		public override void DoPatches()
		{
			QSB.Helper.HarmonyHelper.AddPrefix<PlayerCameraEffectController>("OnStartOfTimeLoop", typeof(WakeUpPatches), nameof(OnStartOfTimeLoopPrefix));
		}
	}
}
