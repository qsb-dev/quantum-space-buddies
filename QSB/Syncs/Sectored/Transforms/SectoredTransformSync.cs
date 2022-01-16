using Mirror;
using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Syncs.Sectored.Transforms
{
	public abstract class SectoredTransformSync : BaseSectoredSync
	{
		protected override bool DestroyAttachedObject => true;

		protected abstract Transform InitLocalTransform();
		protected abstract Transform InitRemoteTransform();

		protected override Transform InitAttachedTransform()
			=> hasAuthority ? InitLocalTransform() : InitRemoteTransform();

		protected override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);

			if (transform.position == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {LogName} at (0,0,0)!", MessageType.Warning);
			}
		}

		protected override bool UpdateTransform()
		{
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
