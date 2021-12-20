using UnityEngine;

namespace QSB.Utility
{
	public static class RelativeTransformUtil
	{
		public static Vector3 ToRelPos(this Transform reference, Vector3 pos) => reference.InverseTransformPoint(pos);
		public static Vector3 FromRelPos(this Transform reference, Vector3 relPos) => reference.TransformPoint(relPos);
		public static Quaternion ToRelRot(this Transform reference, Quaternion rot) => reference.InverseTransformRotation(rot);
		public static Quaternion FromRelRot(this Transform reference, Quaternion relRot) => reference.TransformRotation(relRot);
		public static Vector3 ToRelVel(this OWRigidbody reference, Vector3 vel, Vector3 pos) => vel - reference.GetPointVelocity(pos);
		public static Vector3 FromRelVel(this OWRigidbody reference, Vector3 relVel, Vector3 pos) => relVel + reference.GetPointVelocity(pos);
		public static Vector3 ToRelAngVel(this OWRigidbody reference, Vector3 angVel) => angVel - reference.GetAngularVelocity();
		public static Vector3 FromRelAngVel(this OWRigidbody reference, Vector3 relAngVel) => relAngVel + reference.GetAngularVelocity();
	}
}
