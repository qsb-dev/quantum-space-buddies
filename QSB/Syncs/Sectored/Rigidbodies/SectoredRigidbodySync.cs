using Mirror;
using OWML.Common;
using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs.Sectored.Rigidbodies
{
	public abstract class SectoredRigidbodySync : BaseSectoredSync
	{
		public override bool DestroyAttachedObject => false;

		public const float PositionMovedThreshold = 0.05f;
		public const float AngleRotatedThreshold = 0.05f;
		public const float VelocityChangeThreshold = 0.05f;
		public const float AngVelocityChangeThreshold = 0.05f;

		protected Vector3 _relativeVelocity;
		protected Vector3 _relativeAngularVelocity;
		protected Vector3 _prevVelocity;
		protected Vector3 _prevAngularVelocity;

		/// <summary>
		/// The previous position of the VISIBLE object, as if parented to the reference.
		/// </summary>
		protected Vector3 _localPrevPosition;

		/// <summary>
		/// The previous rotation of the VISIBLE object, as if parented to the reference.
		/// </summary>
		protected Quaternion _localPrevRotation;

		protected Vector3 _localPrevVelocity;
		protected Vector3 _localPrevAngularVelocity;

		public OWRigidbody AttachedRigidbody { get; set; }

		protected abstract OWRigidbody InitAttachedRigidbody();

		protected override Transform InitAttachedTransform()
		{
			AttachedRigidbody = InitAttachedRigidbody();
			return AttachedRigidbody.transform;
		}

		protected override void UpdatePrevData()
		{
			base.UpdatePrevData();
			_prevVelocity = _relativeVelocity;
			_prevAngularVelocity = _relativeAngularVelocity;
		}

		protected override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_relativeVelocity);
			writer.Write(_relativeAngularVelocity);
		}

		protected override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			_relativeVelocity = reader.ReadVector3();
			_relativeAngularVelocity = reader.ReadVector3();

			if (transform.position == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {LogName} at (0,0,0)!", MessageType.Warning);
			}
		}

		protected void SetValuesToSync()
		{
			if (ReferenceTransform != null)
			{
				transform.position = ReferenceTransform.ToRelPos(AttachedRigidbody.GetPosition());
				transform.rotation = ReferenceTransform.ToRelRot(AttachedRigidbody.GetRotation());
				_relativeVelocity = ReferenceTransform.GetAttachedOWRigidbody().ToRelVel(AttachedRigidbody.GetVelocity(), AttachedRigidbody.GetPosition());
				_relativeAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().ToRelAngVel(AttachedRigidbody.GetAngularVelocity());
			}
			else
			{
				transform.position = Vector3.zero;
				transform.rotation = Quaternion.identity;
				_relativeVelocity = Vector3.zero;
				_relativeAngularVelocity = Vector3.zero;
			}
		}

		protected override bool UpdateTransform()
		{
			if (!base.UpdateTransform())
			{
				return false;
			}

			if (hasAuthority)
			{
				SetValuesToSync();
				return true;
			}

			if (ReferenceTransform == null || transform.position == Vector3.zero)
			{
				return false;
			}

			var targetPos = ReferenceTransform.FromRelPos(transform.position);
			var targetRot = ReferenceTransform.FromRelRot(transform.rotation);

			var positionToSet = targetPos;
			var rotationToSet = targetRot;

			if (UseInterpolation)
			{
				positionToSet = ReferenceTransform.FromRelPos(SmoothPosition);
				rotationToSet = ReferenceTransform.FromRelRot(SmoothRotation);
			}

			var hasMoved = CustomHasMoved(
				transform.position,
				_localPrevPosition,
				transform.rotation,
				_localPrevRotation,
				_relativeVelocity,
				_localPrevVelocity,
				_relativeAngularVelocity,
				_localPrevAngularVelocity);

			_localPrevPosition = transform.position;
			_localPrevRotation = transform.rotation;
			_localPrevVelocity = _relativeVelocity;
			_localPrevAngularVelocity = _relativeAngularVelocity;

			if (!hasMoved)
			{
				return true;
			}

			AttachedRigidbody.MoveToPosition(positionToSet);
			AttachedRigidbody.MoveToRotation(rotationToSet);

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelVel(_relativeVelocity, targetPos);
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelAngVel(_relativeAngularVelocity);

			AttachedRigidbody.SetVelocity(targetVelocity);
			AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);

			return true;
		}

		protected override bool HasChanged()
			=> CustomHasMoved(
				transform.position,
				_prevPosition,
				transform.rotation,
				_prevRotation,
				_relativeVelocity,
				_prevVelocity,
				_relativeAngularVelocity,
				_prevAngularVelocity);

		// OPTIMIZE : optimize by using sqrMagnitude
		private bool CustomHasMoved(
			Vector3 newPosition,
			Vector3 prevPosition,
			Quaternion newRotation,
			Quaternion prevRotation,
			Vector3 newVelocity,
			Vector3 prevVelocity,
			Vector3 newAngVelocity,
			Vector3 prevAngVelocity)
		{
			var displacementMagnitude = (newPosition - prevPosition).magnitude;

			if (displacementMagnitude > PositionMovedThreshold)
			{
				return true;
			}

			if (Quaternion.Angle(newRotation, prevRotation) > AngleRotatedThreshold)
			{
				return true;
			}

			var velocityChangeMagnitude = (newVelocity - prevVelocity).magnitude;
			var angularVelocityChangeMagnitude = (newAngVelocity - prevAngVelocity).magnitude;
			if (velocityChangeMagnitude > VelocityChangeThreshold)
			{
				return true;
			}

			if (angularVelocityChangeMagnitude > AngVelocityChangeThreshold)
			{
				return true;
			}

			return false;
		}
	}
}
