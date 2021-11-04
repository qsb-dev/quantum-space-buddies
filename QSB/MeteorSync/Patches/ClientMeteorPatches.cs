using HarmonyLib;
using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.Patches {
    [HarmonyPatch]
    internal class ClientMeteorPatches : QSBPatch {
        public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

        /// ignore all launches from client
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.FixedUpdate))]
        public static bool MeteorLauncher_FixedUpdate(MeteorLauncher __instance) {
            if (__instance._launchedMeteors != null) {
                for (var index = __instance._launchedMeteors.Count - 1; index >= 0; --index) {
                    if (__instance._launchedMeteors[index] == null)
                        __instance._launchedMeteors.QuickRemoveAt(index);
                    else if (__instance._launchedMeteors[index].isSuspended) {
                        __instance._meteorPool.Add(__instance._launchedMeteors[index]);
                        __instance._launchedMeteors.QuickRemoveAt(index);
                    }
                }
            }

            if (__instance._launchedDynamicMeteors != null) {
                for (var index = __instance._launchedDynamicMeteors.Count - 1; index >= 0; --index) {
                    if (__instance._launchedDynamicMeteors[index] == null)
                        __instance._launchedDynamicMeteors.QuickRemoveAt(index);
                    else if (__instance._launchedDynamicMeteors[index].isSuspended) {
                        __instance._dynamicMeteorPool.Add(__instance._launchedDynamicMeteors[index]);
                        __instance._launchedDynamicMeteors.QuickRemoveAt(index);
                    }
                }
            }

            // if (!__instance._initialized || Time.time <= __instance._lastLaunchTime + (double)__instance._launchDelay)
            //     return;
            // if (!__instance._areParticlesPlaying) {
            //     __instance._areParticlesPlaying = true;
            //     foreach (var particleSystem in __instance._launchParticles)
            //         particleSystem.Play();
            // }
            //
            // if (Time.time <= __instance._lastLaunchTime + (double)__instance._launchDelay + 2.29999995231628)
            //     return;
            // __instance.LaunchMeteor();
            // __instance._lastLaunchTime = Time.time;
            // __instance._launchDelay = Random.Range(__instance._minInterval, __instance._maxInterval);
            // __instance._areParticlesPlaying = false;
            // foreach (var particleSystem in __instance._launchParticles)
            //     particleSystem.Stop();

            return false;
        }

        /// launch with precalculated values
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MeteorLauncher), nameof(MeteorLauncher.LaunchMeteor))]
        public static bool MeteorLauncher_LaunchMeteor(MeteorLauncher __instance) {
            var qsbLauncher = QSBWorldSync.GetWorldFromUnity<QSBMeteorLauncher>(__instance);
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

            if (!(meteorController != null)) return false;
            var linearVelocity = __instance._parentBody.GetPointVelocity(__instance.transform.position) +
                                 __instance.transform.TransformDirection(__instance._launchDirection) *
                                 qsbLauncher.launchSpeed;
            var angularVelocity = __instance.transform.forward * 2f;
            meteorController.Launch(null, __instance.transform.position, __instance.transform.rotation, linearVelocity,
                angularVelocity);
            QSBWorldSync.GetWorldFromUnity<QSBMeteorController>(meteorController).damage = qsbLauncher.damage;
            if (!__instance._audioSector.ContainsOccupant(DynamicOccupant.Player)) return false;
            __instance._launchSource.pitch = Random.Range(0.4f, 0.6f);
            __instance._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);

            return false;
        }
    }
}
