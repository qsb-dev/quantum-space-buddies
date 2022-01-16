using Mirror;
using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Syncs.Sectored.Transforms
{
	public abstract class SectoredTransformSync : BaseSectoredSync
	{
		public override bool DestroyAttachedObject => true;

		protected abstract Transform InitLocalTransform();
		protected abstract Transform InitRemoteTransform();

		protected override Transform SetAttachedTransform()
			=> hasAuthority ? InitLocalTransform() : InitRemoteTransform();

		protected override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);

			writer.Write(transform.position);
			writer.Write(transform.rotation);
		}

		protected override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);

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
}
