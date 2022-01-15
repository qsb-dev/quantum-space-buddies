using Mirror;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Syncs.Unsectored.Rigidbodies
{
	public abstract class UnsectoredRigidbodySync2 : BaseUnsectoredSync2
	{
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

		protected abstract OWRigidbody GetRigidbody();

		protected override Transform SetAttachedTransform()
		{
			AttachedRigidbody = GetRigidbody();
			return AttachedRigidbody.transform;
		}

		protected override void UpdatePrevData()
		{
			_prevPosition = transform.position;
			_prevRotation = transform.rotation;
			_prevVelocity = _relativeVelocity;
			_prevAngularVelocity = _relativeAngularVelocity;
		}

		protected override void Serialize(NetworkWriter writer, bool initialState)
		{
			base.Serialize(writer, initialState);

			writer.Write(transform.position);
			writer.Write(transform.rotation);
			writer.Write(_relativeVelocity);
			writer.Write(_relativeAngularVelocity);
		}

		protected override void Deserialize(NetworkReader reader, bool initialState)
		{
			base.Deserialize(reader, initialState);

			var pos = reader.ReadVector3();
			var rot = reader.ReadQuaternion();
			var relativeVelocity = reader.ReadVector3();
			var relativeAngularVelocity = reader.ReadVector3();

			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			transform.position = pos;
			transform.rotation = rot;
			_relativeVelocity = relativeVelocity;
			_relativeAngularVelocity = relativeAngularVelocity;
		}

		protected void SetValuesToSync()
		{
			transform.position = ReferenceTransform.ToRelPos(AttachedRigidbody.GetPosition());
			transform.rotation = ReferenceTransform.ToRelRot(AttachedRigidbody.GetRotation());
			_relativeVelocity = ReferenceTransform.GetAttachedOWRigidbody().ToRelVel(AttachedRigidbody.GetVelocity(), AttachedRigidbody.GetPosition());
			_relativeAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().ToRelAngVel(AttachedRigidbody.GetAngularVelocity());
		}

		protected override bool UpdateTransform()
		{
			if (hasAuthority)
			{
				SetValuesToSync();
				return true;
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
		internal bool CustomHasMoved(
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

	public abstract class UnsectoredRigidbodySync : BaseUnsectoredSync<OWRigidbody>
	{
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
		protected abstract OWRigidbody GetRigidbody();

		protected override OWRigidbody SetAttachedObject()
			=> GetRigidbody();

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			base.SerializeTransform(writer, initialState);

			var worldPos = transform.position;
			var worldRot = transform.rotation;
			var relativeVelocity = _relativeVelocity;
			var relativeAngularVelocity = _relativeAngularVelocity;

			writer.Write(worldPos);
			SerializeRotation(writer, worldRot);
			writer.Write(relativeVelocity);
			writer.Write(relativeAngularVelocity);

			_prevPosition = worldPos;
			_prevRotation = worldRot;
			_prevVelocity = relativeVelocity;
			_prevAngularVelocity = relativeAngularVelocity;
		}

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				reader.ReadVector3();
				DeserializeRotation(reader);
				reader.ReadVector3();
				reader.ReadVector3();
				return;
			}

			var pos = reader.ReadVector3();
			var rot = DeserializeRotation(reader);
			var relativeVelocity = reader.ReadVector3();
			var relativeAngularVelocity = reader.ReadVector3();

			if (HasAuthority)
			{
				return;
			}

			transform.position = pos;
			transform.rotation = rot;
			_relativeVelocity = relativeVelocity;
			_relativeAngularVelocity = relativeAngularVelocity;
		}

		protected void SetValuesToSync()
		{
			transform.position = ReferenceTransform.ToRelPos(AttachedObject.GetPosition());
			transform.rotation = ReferenceTransform.ToRelRot(AttachedObject.GetRotation());
			_relativeVelocity = ReferenceTransform.GetAttachedOWRigidbody().ToRelVel(AttachedObject.GetVelocity(), AttachedObject.GetPosition());
			_relativeAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().ToRelAngVel(AttachedObject.GetAngularVelocity());
		}

		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				SetValuesToSync();
				return true;
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

			AttachedObject.MoveToPosition(positionToSet);
			AttachedObject.MoveToRotation(rotationToSet);

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelVel(_relativeVelocity, targetPos);
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelAngVel(_relativeAngularVelocity);

			AttachedObject.SetVelocity(targetVelocity);
			AttachedObject.SetAngularVelocity(targetAngularVelocity);

			return true;
		}

		public override bool HasMoved()
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
		internal bool CustomHasMoved(
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
