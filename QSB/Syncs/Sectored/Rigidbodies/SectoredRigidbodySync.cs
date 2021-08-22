using OWML.Common;
using OWML.Utils;
using QSB.ShipSync;
using QSB.Utility;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Syncs.Sectored.Rigidbodies
{
	public abstract class SectoredRigidbodySync : BaseSectoredSync
	{
		public override bool ShouldReparentAttachedObject => false;

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

		protected override Component SetAttachedObject()
			=> GetRigidbody();

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			base.SerializeTransform(writer, initialState);

			var worldPos = _intermediaryTransform.GetPosition();
			var worldRot = _intermediaryTransform.GetRotation();
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
			base.DeserializeTransform(reader, initialState);

			if (!QSBCore.WorldObjectsReady)
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

			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}

			_intermediaryTransform.SetPosition(pos);
			_intermediaryTransform.SetRotation(rot);
			_relativeVelocity = relativeVelocity;
			_relativeAngularVelocity = relativeAngularVelocity;

			if (_intermediaryTransform.GetPosition() == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {_logName} at (0,0,0)! - Given position was {pos}", MessageType.Warning);
			}
		}

		protected override bool UpdateTransform()
		{
			if (!base.UpdateTransform())
			{
				return false;
			}

			if (HasAuthority)
			{
				_intermediaryTransform.EncodePosition(AttachedObject.transform.position);
				_intermediaryTransform.EncodeRotation(AttachedObject.transform.rotation);
				_relativeVelocity = GetRelativeVelocity();
				_relativeAngularVelocity = (AttachedObject as OWRigidbody).GetRelativeAngularVelocity(ReferenceTransform.GetAttachedOWRigidbody());
				return true;
			}

			var targetPos = _intermediaryTransform.GetTargetPosition_Unparented();
			var targetRot = _intermediaryTransform.GetTargetRotation_Unparented();

			if (targetPos == Vector3.zero || _intermediaryTransform.GetTargetPosition_ParentedToReference() == Vector3.zero)
			{
				return false;
			}

			Vector3 positionToSet = targetPos;
			Quaternion rotationToSet = targetRot;

			if (UseInterpolation)
			{
				positionToSet = SmartSmoothDamp(AttachedObject.transform.position, targetPos);
				rotationToSet = QuaternionHelper.SmoothDamp(AttachedObject.transform.rotation, targetRot, ref _rotationSmoothVelocity, SmoothTime);
			}

			var hasMoved = CustomHasMoved(
				_intermediaryTransform.GetTargetPosition_ParentedToReference(),
				_localPrevPosition,
				_intermediaryTransform.GetTargetRotation_ParentedToReference(),
				_localPrevRotation,
				_relativeVelocity,
				_localPrevVelocity,
				_relativeAngularVelocity,
				_localPrevAngularVelocity);

			_localPrevPosition = _intermediaryTransform.GetTargetPosition_ParentedToReference();
			_localPrevRotation = _intermediaryTransform.GetTargetRotation_ParentedToReference();
			_localPrevVelocity = _relativeVelocity;
			_localPrevAngularVelocity = _relativeAngularVelocity;

			if (!hasMoved)
			{
				return true;
			}

			//(AttachedObject as OWRigidbody).SetPosition(positionToSet);
			//(AttachedObject as OWRigidbody).SetRotation(rotationToSet);

			(AttachedObject as OWRigidbody).MoveToPosition(positionToSet);
			(AttachedObject as OWRigidbody).MoveToRotation(rotationToSet);

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().GetPointVelocity(targetPos) + _relativeVelocity;
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().GetAngularVelocity() + _relativeAngularVelocity;

			SetVelocity(AttachedObject as OWRigidbody, targetVelocity);
			(AttachedObject as OWRigidbody).SetAngularVelocity(targetAngularVelocity);

			return true;
		}

		public override bool HasMoved() 
			=> CustomHasMoved(
				_intermediaryTransform.GetPosition(),
				_prevPosition,
				_intermediaryTransform.GetRotation(),
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

		// TODO : why? isn't owrigidbody.setvelocity the same...? :P
		protected void SetVelocity(OWRigidbody rigidbody, Vector3 relativeVelocity)
		{
			var isRunningKinematic = rigidbody.RunningKinematicSimulation();
			var currentVelocity = rigidbody.GetValue<Vector3>("_currentVelocity");

			if (isRunningKinematic)
			{
				var kinematicRigidbody = rigidbody.GetValue<KinematicRigidbody>("_kinematicRigidbody");
				kinematicRigidbody.velocity = relativeVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameWorldVelocity();
			}
			else
			{
				var normalRigidbody = rigidbody.GetValue<Rigidbody>("_rigidbody");
				normalRigidbody.velocity = relativeVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameWorldVelocity();
			}

			rigidbody.SetValue("_lastVelocity", currentVelocity);
			rigidbody.SetValue("_currentVelocity", relativeVelocity);
		}

		public float GetVelocityChangeMagnitude()
			=> (_relativeVelocity - _prevVelocity).magnitude;

		public Vector3 GetRelativeVelocity()
		{
			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Error - Trying to get relative velocity when AttachedObject is null.", MessageType.Error);
				return Vector3.zero;
			}

			if (ReferenceTransform == null)
			{
				DebugLog.ToConsole($"Error - Trying to get relative velocity when ReferenceTransform is null. ({AttachedObject.name})", MessageType.Error);
				return Vector3.zero;
			}

			var attachedRigid = ReferenceTransform.GetAttachedOWRigidbody();
			if (attachedRigid == null)
			{
				DebugLog.ToConsole($"Error - ReferenceTransform ({ReferenceTransform.name}) on {AttachedObject.name} has no attached OWRigidBody.", MessageType.Error);
				return Vector3.zero;
			}

			var pointVelocity = attachedRigid.GetPointVelocity(AttachedObject.transform.position);
			return (AttachedObject as OWRigidbody).GetVelocity() - pointVelocity;
		}
	}
}
