using Mirror;
using OWML.Common;
using QSB.Utility;
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

		protected override void Deserialize(NetworkReader reader, bool initialState)
		{
			base.Deserialize(reader, initialState);

			if (transform.position == Vector3.zero)
			{
			}
		}

		protected override void GetFromAttached()
		{
			GetFromSector();

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
		}

		protected override void ApplyToAttached()
		{
			ApplyToSector();

			if (ReferenceTransform == null || transform.position == Vector3.zero)
			{
				return;
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
		}
	}
}
