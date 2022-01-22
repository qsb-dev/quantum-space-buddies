using QSB.Utility;
using UnityEngine;

namespace QSB.Syncs.Sectored.Transforms
{
	public abstract class SectoredTransformSync : BaseSectoredSync
	{
		protected abstract Transform InitLocalTransform();
		protected abstract Transform InitRemoteTransform();

		protected sealed override Transform InitAttachedTransform()
			=> hasAuthority ? InitLocalTransform() : InitRemoteTransform();

		protected override void GetFromAttached()
		{
			GetFromSector();
			if (!ReferenceTransform)
			{
				return;
			}

			transform.position = ReferenceTransform.ToRelPos(AttachedTransform.position);
			transform.rotation = ReferenceTransform.ToRelRot(AttachedTransform.rotation);
		}

		protected override void ApplyToAttached()
		{
			ApplyToSector();
			if (!ReferenceTransform)
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
