﻿using QSB.JellyfishSync.WorldObjects;
using QSB.Syncs;
using QSB.Syncs.Unsectored.Rigidbodies;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.JellyfishSync.TransformSync
{
	public class JellyfishTransformSync : UnsectoredRigidbodySync
	{
		public override bool IsReady => WorldObjectManager.AllObjectsAdded;
		public override bool UseInterpolation => false;

		public int ObjectId = -1;
		private QSBJellyfish _qsbJellyfish;

		protected override OWRigidbody GetRigidbody()
			=> _qsbJellyfish.AttachedObject._jellyfishBody;

		public override float GetNetworkSendInterval() => 10;

		protected override void Init()
		{
			_qsbJellyfish = QSBWorldSync.GetWorldFromId<QSBJellyfish>(ObjectId);
			_qsbJellyfish.TransformSync = this;

			base.Init();
			SetReferenceTransform(_qsbJellyfish.AttachedObject._planetBody.transform);
		}

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			base.SerializeTransform(writer, initialState);

			if (initialState)
			{
				writer.Write(ObjectId);
			}

			if (!WorldObjectManager.AllObjectsReady)
			{
				writer.Write(false);
				return;
			}

			_qsbJellyfish.Align = true;
			writer.Write(_qsbJellyfish.IsRising);
		}

		private bool _shouldUpdate;

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			base.DeserializeTransform(reader, initialState);

			if (initialState)
			{
				ObjectId = reader.ReadInt32();
			}

			if (!WorldObjectManager.AllObjectsReady || HasAuthority)
			{
				reader.ReadBoolean();
				return;
			}

			_qsbJellyfish.Align = false;
			_qsbJellyfish.IsRising = reader.ReadBoolean();
			_shouldUpdate = true;
		}

		/// replacement using SetPosition/Rotation instead of Move
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

			((OWRigidbody)AttachedObject).SetPosition(positionToSet);
			((OWRigidbody)AttachedObject).SetRotation(rotationToSet);

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().DecodeVel(_relativeVelocity, targetPos);
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().DecodeAngVel(_relativeAngularVelocity);

			((OWRigidbody)AttachedObject).SetVelocity(targetVelocity);
			((OWRigidbody)AttachedObject).SetAngularVelocity(targetAngularVelocity);

			return true;
		}


		protected override void OnRenderObject()
		{
			if (!WorldObjectManager.AllObjectsReady
				|| !QSBCore.ShowLinesInDebug
				|| !IsReady
				|| ReferenceTransform == null
				|| ((OWRigidbody)AttachedObject).IsSuspended())
			{
				return;
			}

			base.OnRenderObject();

			var jellyfish = _qsbJellyfish.AttachedObject;
			var position = ReferenceTransform.position;
			var dir = Vector3.Normalize(jellyfish.transform.position - position);
			// Popcron.Gizmos.Line(position + dir * jellyfish._lowerLimit, position + dir * jellyfish._upperLimit, Color.magenta);
			Popcron.Gizmos.Sphere(position + dir * jellyfish._lowerLimit, 10f, Color.magenta);
			Popcron.Gizmos.Sphere(position + dir * jellyfish._upperLimit, 10f, Color.magenta);
		}
	}
}
