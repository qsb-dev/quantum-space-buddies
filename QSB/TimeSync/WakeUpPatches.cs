using OWML.ModHelper.Events;

namespace QSB.TimeSync
{
    public static class WakeUpPatches
    {
        public static void AddPatches()
        {
            QSB.Helper.HarmonyHelper.AddPrefix<PlayerCameraEffectController>("OnStartOfTimeLoop", typeof(WakeUpPatches), nameof(OnStartOfTimeLoopPrefix));
        }

        public static bool OnStartOfTimeLoopPrefix(ref PlayerCameraEffectController __instance)
        {
            if (__instance.gameObject.CompareTag("MainCamera") && QSBSceneManager.CurrentScene != OWScene.EyeOfTheUniverse)
            {
                __instance.Invoke("WakeUp");
            }
            return false;
        }
    }
}
