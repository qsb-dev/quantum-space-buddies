using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs
{
	/// encode = absolute to relative
	/// decode = relative to absolute
	public static class TransformSyncUtil
	{
		public static Vector3 EncodePos(this Transform reference, Vector3 pos) => reference.InverseTransformPoint(pos);
		public static Vector3 DecodePos(this Transform reference, Vector3 relPos) => reference.TransformPoint(relPos);
		public static Quaternion EncodeRot(this Transform reference, Quaternion rot) => reference.InverseTransformRotation(rot);
		public static Quaternion DecodeRot(this Transform reference, Quaternion relRot) => reference.TransformRotation(relRot);
		public static Vector3 EncodeVel(this OWRigidbody reference, Vector3 vel, Vector3 pos) => vel - reference.GetPointVelocity(pos);
		public static Vector3 DecodeVel(this OWRigidbody reference, Vector3 relVel, Vector3 pos) => relVel + reference.GetPointVelocity(pos);
		public static Vector3 EncodeAngVel(this OWRigidbody reference, Vector3 angVel) => angVel - reference.GetAngularVelocity();
		public static Vector3 DecodeAngVel(this OWRigidbody reference, Vector3 relAngVel) => relAngVel + reference.GetAngularVelocity();
	}
}
