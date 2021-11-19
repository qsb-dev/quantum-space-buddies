using OWML.Common;
using QSB.Utility;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Syncs.Sectored.Transforms
{
	public abstract class SectoredTransformSync : BaseSectoredSync
	{
		public override bool ShouldReparentAttachedObject => true;

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
			base.DeserializeTransform(reader, initialState);

			if (!QSBCore.WorldObjectsReady)
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
				DebugLog.ToConsole($"Warning - {LogName} at (0,0,0)! - Given position was {pos}", MessageType.Warning);
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
				if (ReferenceTransform != null)
				{
					transform.position = ReferenceTransform.EncodePos(AttachedObject.transform.position);
					transform.rotation = ReferenceTransform.EncodeRot(AttachedObject.transform.rotation);
				}
				else
				{
					transform.position = Vector3.zero;
					transform.rotation = Quaternion.identity;
				}

				return true;
			}

			var targetPos = transform.position;
			var targetRot = transform.rotation;
			if (targetPos != Vector3.zero && ReferenceTransform.DecodePos(transform.position) != Vector3.zero)
			{
				if (UseInterpolation)
				{
					AttachedObject.transform.localPosition = SmartSmoothDamp(AttachedObject.transform.localPosition, targetPos);
					AttachedObject.transform.localRotation = QuaternionHelper.SmoothDamp(AttachedObject.transform.localRotation, targetRot, ref _rotationSmoothVelocity, SmoothTime);
				}
				else
				{
					AttachedObject.transform.localPosition = targetPos;
					AttachedObject.transform.localRotation = targetRot;
				}
			}

			return true;
		}
	}
}
