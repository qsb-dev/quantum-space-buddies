using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.TimeSync
{
    public static class WakeUpPatches
    {
        public static void AddPatches()
        {
            QSB.Helper.HarmonyHelper.AddPrefix<PlayerCameraEffectController>("OnStartOfTimeLoop", typeof(WakeUpPatches), nameof(WakeUpPatches.OnStartOfTimeLoopPrefix));
        }

        public static bool OnStartOfTimeLoopPrefix(ref PlayerCameraEffectController __instance)
        {
            if (__instance.gameObject.CompareTag("MainCamera") && LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse)
            {
                __instance.Call("WakeUp");
            }
            return false;
        }
    }
}
