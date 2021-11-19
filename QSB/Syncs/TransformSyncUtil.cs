using OWML.Common;
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

	public class _IntermediaryTransform
	{
		private readonly Transform _attachedTransform;
		private Transform _referenceTransform;

		public _IntermediaryTransform(Transform transform)
			=> _attachedTransform = transform;

		/// <summary>
		/// Get the world position of this INVISIBLE transform.
		/// </summary>
		public Vector3 GetPosition()
			=> _attachedTransform.position;

		/// <summary>
		/// Set the world position of this INVISIBLE transform.
		/// </summary>
		public void SetPosition(Vector3 relPos)
			=> _attachedTransform.position = relPos;

		/// <summary>
		/// Get the world rotation of this INVISIBLE transform.
		/// </summary>
		public Quaternion GetRotation()
			=> _attachedTransform.rotation;

		/// <summary>
		/// Set the world rotation of this INVISIBLE transform.
		/// </summary>
		public void SetRotation(Quaternion relRot)
			=> _attachedTransform.rotation = relRot;

		/// <summary>
		/// Sets the reference transform - what transform this transform is syncing to.
		/// </summary>
		/// <param name="sector">The new reference sector.</param>
		public void SetReferenceTransform(Transform reference)
			=> _referenceTransform = reference;

		/// <summary>
		/// Returns the reference transform - what transform this transform is syncing to.
		/// </summary>
		public Transform GetReferenceTransform()
			=> _referenceTransform;

		/// <summary>
		/// Sets the position of the INVISIBLE transform to be correct, according to the reference sector and the position of the VISIBLE transform.
		/// </summary>
		/// <param name="pos">The world position of the VISIBLE transform.</param>
		public void EncodePos(Vector3 pos)
		{
			if (_referenceTransform == null)
			{
				DebugLog.ToConsole($"Error - _referenceTransform has not been set for {_attachedTransform.name}", MessageType.Error);
				return;
			}

			SetPosition(_referenceTransform.InverseTransformPoint(pos));
		}

		/// <summary>
		/// Sets the rotation of the INVISIBLE transform to be correct, according to the reference sector and the rotation of the VISIBLE transform.
		/// </summary>
		/// <param name="worldPosition">The world rotation of the VISIBLE transform.</param>
		public void EncodeRot(Quaternion rot)
		{
			if (_referenceTransform == null)
			{
				DebugLog.ToConsole($"Error - _referenceTransform has not been set for {_attachedTransform.name}", MessageType.Error);
				return;
			}

			SetRotation(_referenceTransform.InverseTransformRotation(rot));
		}

		/// <summary>
		/// Returns the local position the VISIBLE transform should be set to, from the INVISIBLE transform.
		/// </summary>
		public Vector3 GetPosition2()
			=> GetPosition();

		/// <summary>
		/// Returns the local rotation the VISIBLE transform should be set to, from the INVISIBLE transform.
		/// </summary>
		public Quaternion GetRotation2()
			=> GetRotation();

		/// <summary>
		/// Returns the world position the VISIBLE transform should be set to, from the INVISIBLE transform.
		/// </summary>
		public Vector3 DecodePos()
		{
			if (_referenceTransform == null)
			{
				DebugLog.ToConsole($"Error - _referenceTransform has not been set for {_attachedTransform.name}", MessageType.Error);
				return Vector3.zero;
			}

			return _referenceTransform.TransformPoint(GetPosition());
		}

		/// <summary>
		/// Returns the world rotation the VISIBLE transform should be set to, from the INVISIBLE transform.
		/// </summary>
		public Quaternion DecodeRot()
		{
			if (_referenceTransform == null)
			{
				DebugLog.ToConsole($"Error - _referenceTransform has not been set for {_attachedTransform.name}", MessageType.Error);
				return Quaternion.identity;
			}

			return _referenceTransform.TransformRotation(GetRotation());
		}
	}
}
