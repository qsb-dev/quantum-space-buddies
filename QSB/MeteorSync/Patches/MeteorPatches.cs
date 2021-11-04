using HarmonyLib;
using QSB.MeteorSync.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.Patches {
    [HarmonyPatch]
    internal class MeteorPatches : QSBPatch {
        public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;


        /// use precalculated damage value when impacting
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Impact))]
        public static bool MeteorController_Impact(MeteorController __instance,
            GameObject hitObject, Vector3 impactPoint, Vector3 impactVel) {
            var componentInParent = hitObject.GetComponentInParent<FragmentIntegrity>();
            if (componentInParent != null) {
                var damage = QSBWorldSync.GetWorldFromUnity<QSBMeteorController>(__instance).damage;
                // DebugLog.DebugWrite($"meteor impact with precalculated damage {damage}");
                if (!componentInParent.GetIgnoreMeteorDamage())
                    componentInParent.AddDamage(damage);
                else if (componentInParent.GetParentFragment() != null &&
                         !componentInParent.GetParentFragment().GetIgnoreMeteorDamage())
                    componentInParent.GetParentFragment().AddDamage(damage);
            }

            MeteorImpactMapper.RecordImpact(impactPoint, componentInParent);
            __instance._intactRenderer.enabled = false;
            __instance._impactLight.enabled = true;
            __instance._impactLight.intensity = __instance._impactLightCurve.Evaluate(0.0f);
            var quaternion = Quaternion.LookRotation(impactVel);
            foreach (var impactParticle in __instance._impactParticles) {
                impactParticle.transform.rotation = quaternion;
                impactParticle.Play();
            }

            __instance._impactSource.PlayOneShot(AudioType.BH_MeteorImpact);
            foreach (var owCollider in __instance._owColliders)
                owCollider.SetActivation(false);

            __instance._owRigidbody.MakeKinematic();
            __instance.transform.SetParent(hitObject.GetAttachedOWRigidbody().transform);
            FragmentSurfaceProxy.UntrackMeteor(__instance);
            FragmentCollisionProxy.UntrackMeteor(__instance);
            __instance._ignoringCollisions = false;
            __instance._hasImpacted = true;
            __instance._impactTime = Time.time;

            return false;
        }
    }
}
