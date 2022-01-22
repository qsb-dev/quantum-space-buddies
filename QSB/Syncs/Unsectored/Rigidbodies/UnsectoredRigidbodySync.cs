using Mirror;
using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs.Unsectored.Rigidbodies
{
	public abstract class UnsectoredRigidbodySync : BaseUnsectoredSync
	{
		protected override bool CheckValid() => base.CheckValid() && !AttachedRigidbody.IsSuspended();

		private const float PositionMovedThreshold = 0.05f;
		private const float AngleRotatedThreshold = 0.05f;
		private const float VelocityChangeThreshold = 0.05f;
		private const float AngVelocityChangeThreshold = 0.05f;

		protected Vector3 _relativeVelocity;
		protected Vector3 _relativeAngularVelocity;
		private Vector3 _prevVelocity;
		private Vector3 _prevAngularVelocity;

		protected OWRigidbody AttachedRigidbody { get; private set; }

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

		protected override void Serialize(NetworkWriter writer, bool initialState)
		{
			base.Serialize(writer, initialState);
			writer.Write(_relativeVelocity);
			writer.Write(_relativeAngularVelocity);
		}

		protected override void Deserialize(NetworkReader reader, bool initialState)
		{
			base.Deserialize(reader, initialState);
			_relativeVelocity = reader.ReadVector3();
			_relativeAngularVelocity = reader.ReadVector3();
		}

		protected override void GetFromAttached()
		{
			transform.position = ReferenceTransform.ToRelPos(AttachedRigidbody.GetPosition());
			transform.rotation = ReferenceTransform.ToRelRot(AttachedRigidbody.GetRotation());
			_relativeVelocity = ReferenceTransform.GetAttachedOWRigidbody().ToRelVel(AttachedRigidbody.GetVelocity(), AttachedRigidbody.GetPosition());
			_relativeAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().ToRelAngVel(AttachedRigidbody.GetAngularVelocity());
		}

		protected override void ApplyToAttached()
		{
			var targetPos = ReferenceTransform.FromRelPos(transform.position);
			var targetRot = ReferenceTransform.FromRelRot(transform.rotation);

			var positionToSet = targetPos;
			var rotationToSet = targetRot;

			if (UseInterpolation)
			{
				positionToSet = ReferenceTransform.FromRelPos(SmoothPosition);
				rotationToSet = ReferenceTransform.FromRelRot(SmoothRotation);
			}

			AttachedRigidbody.MoveToPosition(positionToSet);
			AttachedRigidbody.MoveToRotation(rotationToSet);

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelVel(_relativeVelocity, targetPos);
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelAngVel(_relativeAngularVelocity);

			AttachedRigidbody.SetVelocity(targetVelocity);
			AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);
		}

		protected override bool HasChanged()
		{
			if (Vector3.Distance(transform.position, _prevPosition) > PositionMovedThreshold)
			{
				return true;
			}

			if (Quaternion.Angle(transform.rotation, _prevRotation) > AngleRotatedThreshold)
			{
				return true;
			}

			if (Vector3.Distance(_relativeVelocity, _prevVelocity) > VelocityChangeThreshold)
			{
				return true;
			}

			if (Vector3.Distance(_relativeAngularVelocity, _prevAngularVelocity) > AngVelocityChangeThreshold)
			{
				return true;
			}

			return false;
		}
	}
}
