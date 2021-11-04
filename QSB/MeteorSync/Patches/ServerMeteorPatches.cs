using HarmonyLib;
using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.Patches {
    [HarmonyPatch]
    internal class ServerMeteorPatches : QSBPatch {
        public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

        /// precalculate everything random, send event saying we launched
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.LaunchMeteor))]
        public static bool MeteorLauncher_LaunchMeteor(MeteorLauncher __instance) {
            var qsbLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);
            qsbLauncher.num = __instance._dynamicMeteorPool == null ? 0 :
                __instance._meteorPool == null ? 1 :
                Random.value < (double)__instance._dynamicProbability ? 1 : 0;
            var meteorController = (MeteorController)null;
            if (qsbLauncher.num == 0) {
                if (__instance._meteorPool.Count == 0) {
                    Debug.LogWarning("MeteorLauncher is out of Meteors!", __instance);
                } else {
                    meteorController = __instance._meteorPool[__instance._meteorPool.Count - 1];
                    meteorController.Initialize(__instance.transform, __instance._detectableField,
                        __instance._detectableFluid);
                    __instance._meteorPool.QuickRemoveAt(__instance._meteorPool.Count - 1);
                    __instance._launchedMeteors.Add(meteorController);
                }
            } else if (__instance._dynamicMeteorPool.Count == 0) {
                Debug.LogWarning("MeteorLauncher is out of Dynamic Meteors!", __instance);
            } else {
                meteorController = __instance._dynamicMeteorPool[__instance._dynamicMeteorPool.Count - 1];
                meteorController.Initialize(__instance.transform, null, null);
                __instance._dynamicMeteorPool.QuickRemoveAt(__instance._dynamicMeteorPool.Count - 1);
                __instance._launchedDynamicMeteors.Add(meteorController);
            }

            if (!(meteorController != null))
                return false;
            qsbLauncher.launchSpeed = Random.Range(__instance._minLaunchSpeed, __instance._maxLaunchSpeed);
            var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) +
                                 __instance.transform.TransformDirection(__instance._launchDirection) *
                                 qsbLauncher.launchSpeed;
            var angularVelocity = __instance.transform.forward * 2f;
            meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity,
                angularVelocity);
            qsbLauncher.damage = Random.Range(meteorController._minDamage, meteorController._maxDamage);
            QSBWorldSync.GetWorldFromUnity<QSBMeteorController>(meteorController).damage = qsbLauncher.damage;
            QSBEventManager.FireEvent(EventNames.QSBMeteorLaunch, qsbLauncher.ObjectId);
            if (!__instance._audioSector.ContainsOccupant(DynamicOccupant.Player))
                return false;
            __instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
            __instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);

            return false;
        }
    }
}
