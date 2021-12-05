using QSB.Syncs;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.TornadoSync.TransformSync
{
	public class OccasionalTransformSync : UnsectoredRigidbodySync
	{
		public override bool IsReady => WorldObjectManager.AllObjectsReady;
		public override bool UseInterpolation => false;

		protected override OWRigidbody GetRigidbody() => CenterOfTheUniverse.s_rigidbodies[_bodyIndex];

		private int _bodyIndex = -1;
		private int _refBodyIndex = -1;

		public void InitBodyIndexes(OWRigidbody body, OWRigidbody refBody)
		{
			_bodyIndex = CenterOfTheUniverse.s_rigidbodies.IndexOf(body);
			_refBodyIndex = CenterOfTheUniverse.s_rigidbodies.IndexOf(refBody);
		}

		public override float GetNetworkSendInterval() => 1;

		protected override void Init()
		{
			base.Init();
			SetReferenceTransform(CenterOfTheUniverse.s_rigidbodies[_refBodyIndex].transform);

			// to prevent change in rotation/angvel
			if (!HasAuthority && AttachedObject.TryGetComponent<AlignWithDirection>(out var align))
			{
				align.enabled = false;
			}
		}

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			base.SerializeTransform(writer, initialState);

			if (initialState)
			{
				writer.Write(_bodyIndex);
				writer.Write(_refBodyIndex);
			}
		}

		private bool _shouldUpdate;

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			base.DeserializeTransform(reader, initialState);

			if (initialState)
			{
				_bodyIndex = reader.ReadInt32();
				_refBodyIndex = reader.ReadInt32();
			}

			if (!WorldObjectManager.AllObjectsReady || HasAuthority)
			{
				return;
			}

			_shouldUpdate = true;
		}

		/// replacement that handles KinematicRigidbody
		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				SetValuesToSync();
				return true;
			}

			if (!_shouldUpdate)
			{
				return false;
			}

			_shouldUpdate = false;

			var targetPos = ReferenceTransform.DecodePos(transform.position);
			var targetRot = ReferenceTransform.DecodeRot(transform.rotation);

			if (targetPos == Vector3.zero || transform.position == Vector3.zero)
			{
				return false;
			}

			var positionToSet = targetPos;
			var rotationToSet = targetRot;

			if (UseInterpolation)
			{
				positionToSet = SmartSmoothDamp(AttachedObject.transform.position, targetPos);
				rotationToSet = QuaternionHelper.SmoothDamp(AttachedObject.transform.rotation, targetRot, ref _rotationSmoothVelocity, SmoothTime);
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

			if (((OWRigidbody)AttachedObject).RunningKinematicSimulation())
			{
				((OWRigidbody)AttachedObject).SetPosition(positionToSet);
				((OWRigidbody)AttachedObject).SetRotation(rotationToSet);
			}
			else
			{
				// does nothing for KinematicRigidbody
				((OWRigidbody)AttachedObject).MoveToPosition(positionToSet);
				((OWRigidbody)AttachedObject).MoveToRotation(rotationToSet);
			}

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().DecodeVel(_relativeVelocity, targetPos);
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().DecodeAngVel(_relativeAngularVelocity);

			((OWRigidbody)AttachedObject).SetVelocity(targetVelocity);
			((OWRigidbody)AttachedObject).SetAngularVelocity(targetAngularVelocity);

			return true;
		}
	}
}
