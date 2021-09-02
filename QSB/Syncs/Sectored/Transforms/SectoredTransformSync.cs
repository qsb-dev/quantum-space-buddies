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

			var worldPos = _intermediaryTransform.GetPosition();
			var worldRot = _intermediaryTransform.GetRotation();
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

			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}

			_intermediaryTransform.SetPosition(pos);
			_intermediaryTransform.SetRotation(rot);

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
				if (ReferenceTransform != null)
				{
					_intermediaryTransform.EncodePosition(AttachedObject.transform.position);
					_intermediaryTransform.EncodeRotation(AttachedObject.transform.rotation);
				}
				else
				{
					_intermediaryTransform.SetPosition(Vector3.zero);
					_intermediaryTransform.SetRotation(Quaternion.identity);
				}
				
				return true;
			}

			var targetPos = _intermediaryTransform.GetTargetPosition_ParentedToReference();
			var targetRot = _intermediaryTransform.GetTargetRotation_ParentedToReference();
			if (targetPos != Vector3.zero && _intermediaryTransform.GetTargetPosition_Unparented() != Vector3.zero)
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
