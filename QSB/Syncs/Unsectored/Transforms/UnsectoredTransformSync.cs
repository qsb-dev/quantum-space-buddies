using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Syncs.Unsectored.Transforms
{
	public abstract class UnsectoredTransformSync : BaseUnsectoredSync
	{
		protected abstract Component InitLocalTransform();
		protected abstract Component InitRemoteTransform();

		protected override Component SetAttachedObject()
			=> HasAuthority ? InitLocalTransform() : InitRemoteTransform();

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			base.SerializeTransform(writer, initialState);

			var worldPos = transform.position;
			var worldRot = transform.rotation;
			writer.Write(worldPos);
			SerializeRotation(writer, worldRot);
			_prevPosition = worldPos;
			_prevRotation = worldRot;
		}

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				reader.ReadVector3();
				DeserializeRotation(reader);
				return;
			}

			var pos = reader.ReadVector3();
			var rot = DeserializeRotation(reader);

			if (HasAuthority)
			{
				return;
			}

			transform.position = pos;
			transform.rotation = rot;

			if (transform.position == Vector3.zero)
			{
				//DebugLog.ToConsole($"Warning - {_logName} at (0,0,0)! - Given position was {pos}", MessageType.Warning);
			}
		}

		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				transform.position = ReferenceTransform.EncodePos(AttachedObject.transform.position);
				transform.rotation = ReferenceTransform.EncodeRot(AttachedObject.transform.rotation);
				return true;
			}

			var targetPos = ReferenceTransform.DecodePos(transform.position);
			var targetRot = ReferenceTransform.DecodeRot(transform.rotation);
			if (targetPos != Vector3.zero && ReferenceTransform.DecodePos(transform.position) != Vector3.zero)
			{
				if (UseInterpolation)
				{
					AttachedObject.transform.position = SmartSmoothDamp(AttachedObject.transform.position, targetPos);
					AttachedObject.transform.rotation = QuaternionHelper.SmoothDamp(AttachedObject.transform.rotation, targetRot, ref _rotationSmoothVelocity, SmoothTime);
				}
				else
				{
					AttachedObject.transform.position = targetPos;
					AttachedObject.transform.rotation = targetRot;
				}
			}
			else if (targetPos == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - TargetPos for {LogName} was (0,0,0).", MessageType.Warning);
			}

			return true;
		}
	}
}
