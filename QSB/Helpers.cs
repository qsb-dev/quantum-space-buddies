using UnityEngine;

namespace QSB
{
    static class Helpers
    {
        public static Quaternion QuaternionSmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
        {
            // account for double-cover
            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;
            // smooth damp (nlerp approx)
            var Result = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
            ).normalized;
            // compute deriv
            var dtInv = 1f / Time.deltaTime;
            deriv.x = (Result.x - rot.x) * dtInv;
            deriv.y = (Result.y - rot.y) * dtInv;
            deriv.z = (Result.z - rot.z) * dtInv;
            deriv.w = (Result.w - rot.w) * dtInv;
            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
        }
    }
}
