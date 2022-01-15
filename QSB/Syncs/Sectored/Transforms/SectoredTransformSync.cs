using Mirror;
using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Syncs.Sectored.Transforms
{
	public abstract class SectoredTransformSync2 : BaseSectoredSync2
	{
		public override bool DestroyAttachedObject => true;

		protected abstract Transform InitLocalTransform();
		protected abstract Transform InitRemoteTransform();

		protected override Transform SetAttachedTransform()
			=> hasAuthority ? InitLocalTransform() : InitRemoteTransform();

		protected override void Serialize(NetworkWriter writer, bool initialState)
		{
			base.Serialize(writer, initialState);

			writer.Write(transform.position);
			writer.Write(transform.rotation);
		}

		protected override void Deserialize(NetworkReader reader, bool initialState)
		{
			base.Deserialize(reader, initialState);

			var pos = reader.ReadVector3();
			var rot = reader.ReadQuaternion();

			if (!WorldObjectManager.AllObjectsReady)
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

			if (hasAuthority)
			{
				if (ReferenceTransform != null)
				{
					transform.position = ReferenceTransform.ToRelPos(AttachedTransform.position);
					transform.rotation = ReferenceTransform.ToRelRot(AttachedTransform.rotation);
				}
				else
				{
					transform.position = Vector3.zero;
					transform.rotation = Quaternion.identity;
				}

				return true;
			}

			if (ReferenceTransform == null || transform.position == Vector3.zero)
			{
				return false;
			}

			if (UseInterpolation)
			{
				AttachedTransform.position = ReferenceTransform.FromRelPos(SmoothPosition);
				AttachedTransform.rotation = ReferenceTransform.FromRelRot(SmoothRotation);
			}
			else
			{
				AttachedTransform.position = ReferenceTransform.FromRelPos(transform.position);
				AttachedTransform.rotation = ReferenceTransform.FromRelRot(transform.rotation);
			}

			return true;
		}
	}

	public abstract class SectoredTransformSync : BaseSectoredSync<Transform>
	{
		public override bool DestroyAttachedObject => true;

		protected abstract Transform InitLocalTransform();
		protected abstract Transform InitRemoteTransform();

		protected override Transform SetAttachedObject()
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
					transform.position = ReferenceTransform.ToRelPos(AttachedObject.position);
					transform.rotation = ReferenceTransform.ToRelRot(AttachedObject.rotation);
				}
				else
				{
					transform.position = Vector3.zero;
					transform.rotation = Quaternion.identity;
				}

				return true;
			}

			if (ReferenceTransform == null || transform.position == Vector3.zero)
			{
				return false;
			}

			if (UseInterpolation)
			{
				AttachedObject.position = ReferenceTransform.FromRelPos(SmoothPosition);
				AttachedObject.rotation = ReferenceTransform.FromRelRot(SmoothRotation);
			}
			else
			{
				AttachedObject.position = ReferenceTransform.FromRelPos(transform.position);
				AttachedObject.rotation = ReferenceTransform.FromRelRot(transform.rotation);
			}

			return true;
		}
	}
}
